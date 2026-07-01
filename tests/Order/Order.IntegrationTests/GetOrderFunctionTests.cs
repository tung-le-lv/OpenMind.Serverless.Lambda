using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using FluentAssertions;
using Order.Api.Features.CreateOrder;
using Order.Api.Features.GetOrder;
using Order.Api.Shared;
using Order.IntegrationTests.Fixtures;
using System.Text.Json;
using System.Text.Json.Serialization;
using Order.Api.Shared.Application.Dtos;
using Xunit;

namespace Order.IntegrationTests;

[Collection("OrderApi")]
public class GetOrderFunctionTests(OrderApiFixture fixture)
{
    private readonly GetOrderFunction _function = new(fixture.Mediator);
    private static readonly TestLambdaContext Context = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task Handler_WhenOrderExists_ShouldReturnOrderDetails()
    {
        var orderId = await CreateOrderAsync("cust-get-1");

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { ["id"] = orderId }
        };

        var response = await _function.Handler(request, Context);

        response.StatusCode.Should().Be(200);
        var body = JsonSerializer.Deserialize<ApiResponse<OrderDto>>(response.Body, JsonOptions);
        body!.Success.Should().BeTrue();
        body.Data!.Id.Should().Be(orderId);
        body.Data.CustomerId.Should().Be("cust-get-1");
    }

    [Fact]
    public async Task Handler_WhenOrderNotFound_ShouldIndicateNotFound()
    {
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { ["id"] = "non-existent-id" }
        };

        var response = await _function.Handler(request, Context);

        response.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_WhenOrderIdNotProvided_ShouldRejectRequest()
    {
        var response = await _function.Handler(new APIGatewayProxyRequest(), Context);

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
