using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Tracing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Shared;
using Order.Api.Shared.Application.Dtos;

namespace Order.Api.Features.GetAllOrders;

public partial class GetAllOrdersFunction(IMediator mediator)
{
    public GetAllOrdersFunction() : this(_serviceProvider.GetRequiredService<IMediator>()) { }

    [LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
    [Logging(CorrelationIdPath = CorrelationIdPaths.ApiGatewayRest)]
    [Tracing]
    public async Task<APIGatewayProxyResponse> Handler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            Logger.LogInformation("Getting all orders");

            var result = await mediator.Send(new GetAllOrdersQuery());
            return ApiResponseHelper.CreateResponse(200, ApiResponse<IEnumerable<OrderDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting all orders");
            return ApiResponseHelper.CreateResponse(500, ApiResponse<IEnumerable<OrderDto>>.ErrorResponse("Internal server error.", [ex.Message]));
        }
    }
}
