using Amazon.Lambda.APIGatewayEvents;
using Order.Application.DTOs;
using System.Text.Json;

namespace Order.Api.Helpers;

internal static class ApiResponseHelper
{
    internal static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    internal static APIGatewayProxyResponse CreateResponse<T>(int statusCode, ApiResponse<T> body) =>
        new()
        {
            StatusCode = statusCode,
            Body = JsonSerializer.Serialize(body, JsonOptions),
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Headers", "Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token" },
                { "Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS" }
            }
        };
}
