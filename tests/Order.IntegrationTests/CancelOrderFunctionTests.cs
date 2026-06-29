using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using FluentAssertions;
using Order.Api.Features.CancelOrder;
using Order.Api.Features.CreateOrder;
using Order.Api.Features.UpdateOrderStatus;
using Order.Api.Domain.Enums;
using Order.Api.Shared;
using Order.IntegrationTests.Fixtures;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Order.IntegrationTests;

[Collection("OrderApi")]
public class CancelOrderFunctionTests(OrderApiFixture fixture)
{
    private readonly CancelOrderFunction _function = new(fixture.Mediator);
    private static readonly TestLambdaContext Context = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task Handler_WhenOrderIsPending_ShouldCancelSuccessfully()
    {
        var orderId = await CreateOrderAsync("cust-cancel-1");

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { ["id"] = orderId }
        };

        var response = await _function.Handler(request, Context);

        response.StatusCode.Should().Be(200);
        var body = JsonSerializer.Deserialize<ApiResponse<string>>(response.Body, JsonOptions);
        body!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_WhenOrderAlreadyShipped_ShouldNotAllowCancellation()
    {
        var orderId = await CreateOrderAsync("cust-cancel-2");
        await fixture.Mediator.Send(new UpdateOrderStatusCommand(orderId, OrderStatus.Confirmed));
        await fixture.Mediator.Send(new UpdateOrderStatusCommand(orderId, OrderStatus.Processing));
        await fixture.Mediator.Send(new UpdateOrderStatusCommand(orderId, OrderStatus.Shipped));

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { ["id"] = orderId }
        };

        var response = await _function.Handler(request, Context);

        response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Handler_WhenOrderNotFound_ShouldIndicateNotFound()
    {
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { ["id"] = "non-existent" }
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
            [new CreateOrderItemDto("p1", "Product 1", 1, 10m)],
            null));
        return result.OrderId!;
    }
}
