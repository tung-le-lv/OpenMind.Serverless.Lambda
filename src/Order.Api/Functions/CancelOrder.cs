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

namespace Order.Api.Functions;

public class CancelOrder
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddOrderServices();
        return services.BuildServiceProvider();
    }

    private readonly IMediator _mediator;

    public CancelOrder()
    {
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
    }

    public CancelOrder(IMediator mediator)
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
            var orderId = request.PathParameters?["id"];
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return ApiResponseHelper.CreateResponse(400, ApiResponse<string>.ErrorResponse("Order ID is required."));
            }

            Logger.LogInformation("Cancelling order {OrderId}", orderId);

            var command = new CancelOrderCommand(orderId);
            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                var statusCode = result.Message?.Contains("not found") == true ? 404 : 400;
                return ApiResponseHelper.CreateResponse(statusCode, ApiResponse<string>.ErrorResponse(result.Message ?? "Failed to cancel order.", result.Errors));
            }

            Metrics.AddMetric("OrderCancelled", 1, MetricUnit.Count);

            return ApiResponseHelper.CreateResponse(200, ApiResponse<string>.SuccessResponse("OK", result.Message));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error cancelling order {OrderId}", request.PathParameters?["id"]);
            return ApiResponseHelper.CreateResponse(500, ApiResponse<string>.ErrorResponse("Internal server error.", [ex.Message]));
        }
    }
}
