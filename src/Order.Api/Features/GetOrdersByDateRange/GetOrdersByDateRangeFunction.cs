using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Tracing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Application.Dtos;
using Order.Api.Shared;
using Order.Api.Shared.Helpers;

namespace Order.Api.Features.GetOrdersByDateRange;

public partial class GetOrdersByDateRangeFunction(IMediator mediator)
{
    public GetOrdersByDateRangeFunction() : this(_serviceProvider.GetRequiredService<IMediator>()) { }

    [LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
    [Logging(LogEvent = false, CorrelationIdPath = CorrelationIdPaths.ApiGatewayRest)]
    [Tracing]
    public async Task<APIGatewayProxyResponse> Handler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            var qp = request.QueryStringParameters;
            var dateParam = qp != null && qp.TryGetValue("date", out var raw) ? raw : null;

            if (!DateOnly.TryParse(dateParam, out var date))
            {
                return ApiResponseHelper.CreateResponse(400, ApiResponse<IEnumerable<OrderDto>>.ErrorResponse("Query parameter 'date' must be a valid date (YYYY-MM-DD)."));
            }

            Logger.LogInformation($"Getting orders for date {date}");

            var result = await mediator.Send(new GetOrdersByDateRangeQuery(date));
            return ApiResponseHelper.CreateResponse(200, ApiResponse<IEnumerable<OrderDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting orders by date range");
            return ApiResponseHelper.CreateResponse(500, ApiResponse<IEnumerable<OrderDto>>.ErrorResponse("Internal server error.", [ex.Message]));
        }
    }
}
