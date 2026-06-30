using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using FluentAssertions;
using Order.Api.Features.CreateOrder;
using Order.Api.Features.UpdateOrderStatus;
using Order.Api.Shared;
using Order.IntegrationTests.Fixtures;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Order.IntegrationTests;

[Collection("OrderApi")]
public class UpdateOrderStatusFunctionTests(OrderApiFixture fixture)
{
    private readonly UpdateOrderStatusFunction _function = new(fixture.Mediator);
    private static readonly TestLambdaContext Context = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task Handler_WhenValidStatusTransition_ShouldUpdateOrderStatus()
    {
        var orderId = await CreateOrderAsync("cust-status-1");

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { ["id"] = orderId },
            Body = JsonSerializer.Serialize(new { status = "Confirmed" }, JsonOptions)
        };

        var response = await _function.Handler(request, Context);

        response.StatusCode.Should().Be(200);
        var body = JsonSerializer.Deserialize<ApiResponse<string>>(response.Body, JsonOptions);
        body!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_WhenTransitionNotAllowed_ShouldRejectStatusChange()
    {
        var orderId = await CreateOrderAsync("cust-status-2");

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { ["id"] = orderId },
            Body = JsonSerializer.Serialize(new { status = "Delivered" }, JsonOptions)
        };

        var response = await _function.Handler(request, Context);

        response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Handler_WhenOrderNotFound_ShouldIndicateNotFound()
    {
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { ["id"] = "non-existent" },
            Body = JsonSerializer.Serialize(new { status = "Confirmed" }, JsonOptions)
        };

        var response = await _function.Handler(request, Context);

        response.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_WhenOrderIdNotProvided_ShouldRejectRequest()
    {
        var response = await _function.Handler(new APIGatewayProxyRequest
        {
            Body = JsonSerializer.Serialize(new { status = "Confirmed" }, JsonOptions)
        }, Context);

        response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Handler_WhenNoRequestBodyProvided_ShouldRejectOrder()
    {
        var response = await _function.Handler(new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { ["id"] = "any-id" }
        }, Context);

        response.StatusCode.Should().Be(400);
    }

    private async Task<string> CreateOrderAsync(string customerId)
    {
        var result = await fixture.Mediator.Send(new CreateOrderCommand(
            customerId,
            [new CreateOrderItemDto("p1", "Product 1", 1, 10m)]));
        return result.OrderId!;
    }
}
