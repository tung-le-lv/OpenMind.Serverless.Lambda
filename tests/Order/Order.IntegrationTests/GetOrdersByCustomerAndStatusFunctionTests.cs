using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using FluentAssertions;
using Order.Api.Application.Dtos;
using Order.Api.Domain.Enums;
using Order.Api.Features.CreateOrder;
using Order.Api.Features.GetOrdersByCustomerAndStatus;
using Order.Api.Features.UpdateOrderStatus;
using Order.Api.Shared;
using Order.IntegrationTests.Fixtures;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Order.IntegrationTests;

[Collection("OrderApi")]
public class GetOrdersByCustomerAndStatusFunctionTests(OrderApiFixture fixture)
{
    private readonly GetOrdersByCustomerAndStatusFunction _function = new(fixture.Mediator);
    private static readonly TestLambdaContext Context = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task Handler_WhenFilteringByCustomerAndStatus_ShouldReturnMatchingOrders()
    {
        var orderId = await CreateOrderAsync("cust-bystatus-1");
        await fixture.Mediator.Send(new UpdateOrderStatusCommand(orderId, OrderStatus.Confirmed));

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                ["customerId"] = "cust-bystatus-1",
                ["status"] = "Confirmed"
            }
        };

        var response = await _function.Handler(request, Context);

        response.StatusCode.Should().Be(200);
        var body = JsonSerializer.Deserialize<ApiResponse<IEnumerable<OrderDto>>>(response.Body, JsonOptions);
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeEmpty();
        body.Data.Should().AllSatisfy(o => o.Status.Should().Be(OrderStatus.Confirmed));
    }

    [Fact]
    public async Task Handler_WhenStatusValueIsInvalid_ShouldRejectRequest()
    {
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                ["customerId"] = "cust-bystatus-2",
                ["status"] = "InvalidStatus"
            }
        };

        var response = await _function.Handler(request, Context);

        response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Handler_WhenCustomerIdNotProvided_ShouldRejectRequest()
    {
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { ["status"] = "Pending" }
        };

        var response = await _function.Handler(request, Context);

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
