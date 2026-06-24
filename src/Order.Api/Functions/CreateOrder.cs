using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Extensions;
using Order.Api.Helpers;
using Order.Application.Commands;
using Order.Application.DTOs;
using System.Text.Json;

namespace Order.Api.Functions;

public class CreateOrder
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddOrderServices();
        return services.BuildServiceProvider();
    }

    private readonly IMediator _mediator;

    public CreateOrder()
    {
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
    }

    public CreateOrder(IMediator mediator)
    {
        _mediator = mediator;
    }

    [LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
    [Logging(LogEvent = true, CorrelationIdPath = CorrelationIdPaths.ApiGatewayRest)]
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

            Logger.LogInformation("Creating order for customer {CustomerId}", command.CustomerId);

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                return ApiResponseHelper.CreateResponse(400, ApiResponse<string>.ErrorResponse(result.Message ?? "Failed to create order.", result.Errors));
            }

            Metrics.AddMetric("OrderCreated", 1, MetricUnit.Count);

            return ApiResponseHelper.CreateResponse(201, ApiResponse<object>.SuccessResponse(
                new { OrderId = result.OrderId },
                result.Message));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating order");
            return ApiResponseHelper.CreateResponse(500, ApiResponse<string>.ErrorResponse("Internal server error.", [ex.Message]));
        }
    }
}
