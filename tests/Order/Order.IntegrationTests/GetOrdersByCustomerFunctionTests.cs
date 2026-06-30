using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using FluentAssertions;
using Order.Api.Application.Dtos;
using Order.Api.Features.CreateOrder;
using Order.Api.Features.GetOrdersByCustomer;
using Order.Api.Shared;
using Order.IntegrationTests.Fixtures;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Order.IntegrationTests;

[Collection("OrderApi")]
public class GetOrdersByCustomerFunctionTests(OrderApiFixture fixture)
{
    private readonly GetOrdersByCustomerFunction _function = new(fixture.Mediator);
    private static readonly TestLambdaContext Context = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task Handler_WhenCustomerHasOrders_ShouldReturnTheirOrders()
    {
        await CreateOrderAsync("cust-bycust-1");
        await CreateOrderAsync("cust-bycust-1");

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { ["customerId"] = "cust-bycust-1" }
        };

        var response = await _function.Handler(request, Context);

        response.StatusCode.Should().Be(200);
        var body = JsonSerializer.Deserialize<ApiResponse<IEnumerable<OrderDto>>>(response.Body, JsonOptions);
        body!.Success.Should().BeTrue();
        body.Data.Should().HaveCountGreaterThanOrEqualTo(2);
        body.Data.Should().AllSatisfy(o => o.CustomerId.Should().Be("cust-bycust-1"));
    }

    [Fact]
    public async Task Handler_WhenCustomerIdNotProvided_ShouldRejectRequest()
    {
        var response = await _function.Handler(new APIGatewayProxyRequest(), Context);

        response.StatusCode.Should().Be(400);
    }

    private async Task CreateOrderAsync(string customerId)
    {
        await fixture.Mediator.Send(new CreateOrderCommand(
            customerId,
            [new CreateOrderItemDto("p1", "Product 1", 1, 10m)]));
    }
}
