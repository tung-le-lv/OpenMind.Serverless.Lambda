using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using FluentAssertions;
using Order.Api.Features.CreateOrder;
using Order.Api.Features.GetAllOrders;
using Order.Api.Shared;
using Order.IntegrationTests.Fixtures;
using System.Text.Json;
using System.Text.Json.Serialization;
using Order.Api.Shared.Application.Dtos;
using Xunit;

namespace Order.IntegrationTests;

[Collection("OrderApi")]
public class GetAllOrdersFunctionTests(OrderApiFixture fixture)
{
    private readonly GetAllOrdersFunction _function = new(fixture.Mediator);
    private static readonly TestLambdaContext Context = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task Handler_ShouldReturnAllOrders()
    {
        await CreateOrderAsync("cust-getall-1");
        await CreateOrderAsync("cust-getall-2");

        var response = await _function.Handler(new APIGatewayProxyRequest(), Context);

        response.StatusCode.Should().Be(200);
        var body = JsonSerializer.Deserialize<ApiResponse<IEnumerable<OrderDto>>>(response.Body, JsonOptions);
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeEmpty();
    }

    private async Task CreateOrderAsync(string customerId)
    {
        await fixture.Mediator.Send(new CreateOrderCommand(
            customerId,
            [new CreateOrderItemDto("p1", "Product 1", 1, 10m)]));
    }
}
