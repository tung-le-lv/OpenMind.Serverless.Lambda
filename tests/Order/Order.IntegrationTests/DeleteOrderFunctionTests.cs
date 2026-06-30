using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using FluentAssertions;
using Order.Api.Features.CreateOrder;
using Order.Api.Features.DeleteOrder;
using Order.Api.Features.GetOrder;
using Order.Api.Shared;
using Order.IntegrationTests.Fixtures;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Order.IntegrationTests;

[Collection("OrderApi")]
public class DeleteOrderFunctionTests(OrderApiFixture fixture)
{
    private readonly DeleteOrderFunction _function = new(fixture.Mediator);
    private static readonly TestLambdaContext Context = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task Handler_WhenOrderExists_ShouldPermanentlyDeleteOrder()
    {
        var orderId = await CreateOrderAsync("cust-delete-1");

        var response = await _function.Handler(new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { ["id"] = orderId }
        }, Context);

        response.StatusCode.Should().Be(200);
        var body = JsonSerializer.Deserialize<ApiResponse<string>>(response.Body, JsonOptions);
        body!.Success.Should().BeTrue();

        var deleted = await fixture.Mediator.Send(new GetOrderQuery(orderId));
        deleted.Should().BeNull();
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
            [new CreateOrderItemDto("p1", "Product 1", 1, 10m)],
            null));
        return result.OrderId!;
    }
}
