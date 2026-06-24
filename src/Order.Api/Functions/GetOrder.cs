using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Tracing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Extensions;
using Order.Api.Helpers;
using Order.Application.DTOs;
using Order.Application.Queries;

namespace Order.Api.Functions;

public class GetOrder
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddOrderServices();
        return services.BuildServiceProvider();
    }

    private readonly IMediator _mediator;

    public GetOrder()
    {
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
    }

    public GetOrder(IMediator mediator)
    {
        _mediator = mediator;
    }

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

            Logger.LogInformation("Getting order {OrderId}", orderId);

            var query = new GetOrderQuery(orderId);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return ApiResponseHelper.CreateResponse(404, ApiResponse<OrderDto>.ErrorResponse($"Order with ID '{orderId}' not found."));
            }

            return ApiResponseHelper.CreateResponse(200, ApiResponse<OrderDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting order {OrderId}", request.PathParameters?["id"]);
            return ApiResponseHelper.CreateResponse(500, ApiResponse<OrderDto>.ErrorResponse("Internal server error.", [ex.Message]));
        }
    }
}
