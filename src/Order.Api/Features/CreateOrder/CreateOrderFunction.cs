using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Shared;
using System.Text.Json;
using Order.Api.Domain.Interfaces;
using Order.Api.Domain.Repositories;
using Order.Api.Infrastructure.EventBus;
using Order.Api.Infrastructure.Repositories;
using Order.Api.Shared.Helpers;

namespace Order.Api.Features.CreateOrder;

public class CreateOrderFunction(IMediator mediator)
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
        services.AddSingleton<IOrderRepository, DynamoDbOrderRepository>();
        if (Environment.GetEnvironmentVariable("USE_LOCAL_EVENT_BUS") == "true")
        {
            services.AddSingleton<IEventBus, InMemoryEventBus>();
        }
        else
        {
            services.AddSingleton<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>();
            services.AddSingleton<IEventBus, SnsEventBus>();
        }
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DynamoDbOrderRepository).Assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient<IRequestHandler<CreateOrderCommand, CreateOrderResult>, CreateOrderHandler>();
        services.AddTransient<IValidator<CreateOrderCommand>, CreateOrderValidator>();
        return services.BuildServiceProvider();
    }

    public CreateOrderFunction() : this(_serviceProvider.GetRequiredService<IMediator>()) { }

    [LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
    [Logging(LogEvent = false, CorrelationIdPath = CorrelationIdPaths.ApiGatewayRest)]
    [Tracing]
    [Metrics(Namespace = "OrderService", CaptureColdStart = true)]
    public async Task<APIGatewayProxyResponse> Handler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Body))
            {
                return ApiResponseHelper.CreateResponse(400, ApiResponse<string>.ErrorResponse("Request body is required."));
            }

            var command = JsonSerializer.Deserialize<CreateOrderCommand>(request.Body, ApiResponseHelper.JsonOptions);
            if (command == null)
            {
                return ApiResponseHelper.CreateResponse(400, ApiResponse<string>.ErrorResponse("Invalid request body."));
            }

            Logger.LogInformation($"Doing creating order for customer {command.CustomerId}");

            var result = await mediator.Send(command);

            if (!result.Success)
            {
                return ApiResponseHelper.CreateResponse(400, ApiResponse<string>.ErrorResponse(result.Message ?? "Failed to create order.", result.Errors));
            }

            Metrics.AddMetric("OrderCreated", 1, MetricUnit.Count);

            return ApiResponseHelper.CreateResponse(201, ApiResponse<object>.SuccessResponse(
                new { OrderId = result.OrderId }, result.Message));
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors.Select(e => e.ErrorMessage).ToList();
            return ApiResponseHelper.CreateResponse(400, ApiResponse<string>.ErrorResponse("Application validation failed.", errors));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating order");
            return ApiResponseHelper.CreateResponse(500, ApiResponse<string>.ErrorResponse("Internal server error.", [ex.Message]));
        }
    }
}
