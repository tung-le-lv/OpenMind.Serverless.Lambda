using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Extensions;
using Order.Application.Commands;
using Order.Application.DTOs;
using System.Text.Json;

namespace Order.Api.Functions;

public class CancelOrder
{
    private readonly IMediator _mediator;
    private readonly JsonSerializerOptions _jsonOptions;

    public CancelOrder()
    {
        var services = new ServiceCollection();
        services.AddOrderServices();
        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public CancelOrder(IMediator mediator)
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
                return CreateResponse(400, ApiResponse<string>.ErrorResponse("Order ID is required."));
            }

            context.Logger.LogInformation($"Cancelling order: {orderId}");

            var command = new CancelOrderCommand(orderId);
            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                var statusCode = result.Message?.Contains("not found") == true ? 404 : 400;
                return CreateResponse(statusCode, ApiResponse<string>.ErrorResponse(result.Message ?? "Failed to cancel order.", result.Errors));
            }

            return CreateResponse(200, ApiResponse<string>.SuccessResponse("OK", result.Message));
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error cancelling order: {ex.Message}");
            return CreateResponse(500, ApiResponse<string>.ErrorResponse("Internal server error.", [ex.Message]));
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
