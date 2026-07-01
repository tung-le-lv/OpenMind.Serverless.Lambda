using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Tracing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Shared;
using Order.Api.Shared.Application.Dtos;

namespace Order.Api.Features.GetOrdersByCustomer;

public partial class GetOrdersByCustomerFunction(IMediator mediator)
{
    public GetOrdersByCustomerFunction() : this(_serviceProvider.GetRequiredService<IMediator>()) { }

    [LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
    [Logging(CorrelationIdPath = CorrelationIdPaths.ApiGatewayRest)]
    [Tracing]
    public async Task<APIGatewayProxyResponse> Handler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            var customerId = request.PathParameters?["customerId"];
            if (string.IsNullOrWhiteSpace(customerId))
            {
                return ApiResponseHelper.CreateResponse(400, ApiResponse<IEnumerable<OrderDto>>.ErrorResponse("Customer ID is required."));
            }

            Logger.LogInformation($"Getting orders for customer {customerId}");

            var result = await mediator.Send(new GetOrdersByCustomerQuery(customerId));
            return ApiResponseHelper.CreateResponse(200, ApiResponse<IEnumerable<OrderDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error getting orders for customer {request.PathParameters?["customerId"]}");
            return ApiResponseHelper.CreateResponse(500, ApiResponse<IEnumerable<OrderDto>>.ErrorResponse("Internal server error.", [ex.Message]));
        }
    }
}
