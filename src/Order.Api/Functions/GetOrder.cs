using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Extensions;
using Order.Application.DTOs;
using Order.Application.Queries;
using System.Text.Json;

namespace Order.Api.Functions;

public class GetOrder
{
    private readonly IMediator _mediator;
    private readonly JsonSerializerOptions _jsonOptions;

    public GetOrder()
    {
        var services = new ServiceCollection();
        services.AddOrderServices();
        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public GetOrder(IMediator mediator)
    {
        _mediator = mediator;
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    [LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
    public async Task<APIGatewayProxyResponse> Handler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            var orderId = request.PathParameters?["id"];
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return CreateResponse(400, ApiResponse<OrderDto>.ErrorResponse("Order ID is required."));
            }

            context.Logger.LogInformation($"Getting order with ID: {orderId}");

            var query = new GetOrderQuery(orderId);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return CreateResponse(404, ApiResponse<OrderDto>.ErrorResponse($"Order with ID '{orderId}' not found."));
            }

            return CreateResponse(200, ApiResponse<OrderDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error getting order: {ex.Message}");
            return CreateResponse(500, ApiResponse<OrderDto>.ErrorResponse("Internal server error.", [ex.Message]));
        }
    }

    private APIGatewayProxyResponse CreateResponse<T>(int statusCode, ApiResponse<T> body)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = statusCode,
            Body = JsonSerializer.Serialize(body, _jsonOptions),
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Headers", "Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token" },
                { "Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS" }
            }
        };
    }
}
