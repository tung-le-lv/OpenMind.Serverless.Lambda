using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Tracing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Application.Dtos;
using Order.Api.Domain.Enums;
using Order.Api.Shared;
using Order.Api.Shared.Helpers;

namespace Order.Api.Features.GetOrdersByCustomerAndStatus;

public partial class GetOrdersByCustomerAndStatusFunction(IMediator mediator)
{
    public GetOrdersByCustomerAndStatusFunction() : this(_serviceProvider.GetRequiredService<IMediator>()) { }

    [LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
    [Logging(LogEvent = false, CorrelationIdPath = CorrelationIdPaths.ApiGatewayRest)]
    [Tracing]
    public async Task<APIGatewayProxyResponse> Handler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            string? customerId = null;
            request.PathParameters?.TryGetValue("customerId", out customerId);
            if (string.IsNullOrWhiteSpace(customerId))
            {
                return ApiResponseHelper.CreateResponse(400, ApiResponse<IEnumerable<OrderDto>>.ErrorResponse("Customer ID is required."));
            }

            var statusParam = request.PathParameters?["status"];
            if (!Enum.TryParse<OrderStatus>(statusParam, ignoreCase: true, out var status))
            {
                return ApiResponseHelper.CreateResponse(400, ApiResponse<IEnumerable<OrderDto>>.ErrorResponse($"Invalid status '{statusParam}'. Valid values: {string.Join(", ", Enum.GetNames<OrderStatus>())}"));
            }

            Logger.LogInformation($"Getting orders for customer {customerId} with status {status}");

            var result = await mediator.Send(new GetOrdersByCustomerAndStatusQuery(customerId, status));
            return ApiResponseHelper.CreateResponse(200, ApiResponse<IEnumerable<OrderDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting orders by customer and status");
            return ApiResponseHelper.CreateResponse(500, ApiResponse<IEnumerable<OrderDto>>.ErrorResponse("Internal server error.", [ex.Message]));
        }
    }
}
