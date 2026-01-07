using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Extensions;
using Order.Application.Commands;
using Order.Application.DTOs;
using Order.Domain.Enums;
using System.Text.Json;

namespace Order.Api.Functions;

public class UpdateOrderStatus
{
    private readonly IMediator _mediator;
    private readonly JsonSerializerOptions _jsonOptions;

    public UpdateOrderStatus()
    {
        var services = new ServiceCollection();
        services.AddOrderServices();
        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public UpdateOrderStatus(IMediator mediator)
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

            if (string.IsNullOrWhiteSpace(request.Body))
            {
                return CreateResponse(400, ApiResponse<string>.ErrorResponse("Request body is required."));
            }

            var updateRequest = JsonSerializer.Deserialize<UpdateStatusRequest>(request.Body, _jsonOptions);
            if (updateRequest == null)
            {
                return CreateResponse(400, ApiResponse<string>.ErrorResponse("Invalid request body."));
            }

            context.Logger.LogInformation($"Updating status for order {orderId} to {updateRequest.Status}");

            var command = new UpdateOrderStatusCommand(orderId, updateRequest.Status);
            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                var statusCode = result.Message?.Contains("not found") == true ? 404 : 400;
                return CreateResponse(statusCode, ApiResponse<string>.ErrorResponse(result.Message ?? "Failed to update status.", result.Errors));
            }

            return CreateResponse(200, ApiResponse<string>.SuccessResponse("OK", result.Message));
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error updating order status: {ex.Message}");
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

    private record UpdateStatusRequest(OrderStatus Status);
}
