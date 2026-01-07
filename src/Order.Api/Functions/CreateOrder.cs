using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Extensions;
using Order.Application.Commands;
using Order.Application.DTOs;
using System.Text.Json;

namespace Order.Api.Functions;

public class CreateOrder
{
    private readonly IMediator _mediator;
    private readonly JsonSerializerOptions _jsonOptions;

    public CreateOrder()
    {
        var services = new ServiceCollection();
        services.AddOrderServices();
        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    // Constructor for testing
    public CreateOrder(IMediator mediator)
    {
        _mediator = mediator;
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    [LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
    public async Task<APIGatewayProxyResponse> Handler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Body))
            {
                return CreateResponse(400, ApiResponse<string>.ErrorResponse("Request body is required."));
            }

            var command = JsonSerializer.Deserialize<CreateOrderCommand>(request.Body, _jsonOptions);
            if (command == null)
            {
                return CreateResponse(400, ApiResponse<string>.ErrorResponse("Invalid request body."));
            }

            context.Logger.LogInformation($"Creating order for customer: {command.CustomerId}");

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                return CreateResponse(400, ApiResponse<string>.ErrorResponse(result.Message ?? "Failed to create order.", result.Errors));
            }

            return CreateResponse(201, ApiResponse<object>.SuccessResponse(
                new { OrderId = result.OrderId },
                result.Message));
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error creating order: {ex.Message}");
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
