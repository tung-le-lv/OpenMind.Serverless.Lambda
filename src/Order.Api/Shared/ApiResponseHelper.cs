using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;

namespace Order.Api.Shared;

internal static class ApiResponseHelper
{
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

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
