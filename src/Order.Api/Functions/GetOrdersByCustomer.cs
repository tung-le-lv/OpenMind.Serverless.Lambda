using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Extensions;
using Order.Application.DTOs;
using Order.Application.Queries;
using System.Text.Json;

namespace Order.Api.Functions;

public class GetOrdersByCustomer
{
    private readonly IMediator _mediator;
    private readonly JsonSerializerOptions _jsonOptions;

    public GetOrdersByCustomer()
    {
        var services = new ServiceCollection();
        services.AddOrderServices();
        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public GetOrdersByCustomer(IMediator mediator)
    {
        _mediator = mediator;
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    [LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
    public async Task<APIGatewayProxyResponse> Handler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            var customerId = request.PathParameters?["customerId"];
            if (string.IsNullOrWhiteSpace(customerId))
            {
                return CreateResponse(400, ApiResponse<IEnumerable<OrderDto>>.ErrorResponse("Customer ID is required."));
            }

            context.Logger.LogInformation($"Getting orders for customer: {customerId}");

            var query = new GetOrdersByCustomerQuery(customerId);
            var result = await _mediator.Send(query);

            return CreateResponse(200, ApiResponse<IEnumerable<OrderDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error getting orders by customer: {ex.Message}");
            return CreateResponse(500, ApiResponse<IEnumerable<OrderDto>>.ErrorResponse("Internal server error.", [ex.Message]));
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
