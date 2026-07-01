using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Tracing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Shared;
using Order.Api.Shared.Application.Dtos;

namespace Order.Api.Features.GetOrder;

public partial class GetOrderFunction(IMediator mediator)
{
    public GetOrderFunction() : this(_serviceProvider.GetRequiredService<IMediator>()) { }

    [LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
    [Logging(CorrelationIdPath = CorrelationIdPaths.ApiGatewayRest)]
    [Tracing]
    public async Task<APIGatewayProxyResponse> Handler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            var orderId = request.PathParameters?["id"];
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return ApiResponseHelper.CreateResponse(400, ApiResponse<OrderDto>.ErrorResponse("Order ID is required."));
            }

            Logger.LogInformation($"Getting order {orderId}");

            var result = await mediator.Send(new GetOrderQuery(orderId));

            if (result == null)
            {
                return ApiResponseHelper.CreateResponse(404, ApiResponse<OrderDto>.ErrorResponse($"Order with ID '{orderId}' not found."));
            }

            return ApiResponseHelper.CreateResponse(200, ApiResponse<OrderDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error getting order {request.PathParameters?["id"]}");
            return ApiResponseHelper.CreateResponse(500, ApiResponse<OrderDto>.ErrorResponse("Internal server error.", [ex.Message]));
        }
    }
}
