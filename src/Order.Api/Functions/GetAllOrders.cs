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

public class GetAllOrders
{
    private static readonly ServiceProvider _serviceProvider = BuildServiceProvider();

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddOrderServices();
        return services.BuildServiceProvider();
    }

    private readonly IMediator _mediator;

    public GetAllOrders()
    {
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
    }

    public GetAllOrders(IMediator mediator)
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
            Logger.LogInformation("Getting all orders");

            var query = new GetAllOrdersQuery();
            var result = await _mediator.Send(query);

            return ApiResponseHelper.CreateResponse(200, ApiResponse<IEnumerable<OrderDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting all orders");
            return ApiResponseHelper.CreateResponse(500, ApiResponse<IEnumerable<OrderDto>>.ErrorResponse("Internal server error.", [ex.Message]));
        }
    }
}
