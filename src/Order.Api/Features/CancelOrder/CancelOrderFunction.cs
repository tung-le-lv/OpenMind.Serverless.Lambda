using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Shared;

namespace Order.Api.Features.CancelOrder;

public partial class CancelOrderFunction(IMediator mediator)
{
    public CancelOrderFunction() : this(_serviceProvider.GetRequiredService<IMediator>()) { }

    [LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
    [Logging(LogEvent = false, CorrelationIdPath = CorrelationIdPaths.ApiGatewayRest)]
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

            Logger.LogInformation($"Cancelling order {orderId}");

            var result = await mediator.Send(new CancelOrderCommand(orderId));

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
            Logger.LogError(ex, "Error cancelling order");
            return ApiResponseHelper.CreateResponse(500, ApiResponse<string>.ErrorResponse("Internal server error.", [ex.Message]));
        }
    }
}
