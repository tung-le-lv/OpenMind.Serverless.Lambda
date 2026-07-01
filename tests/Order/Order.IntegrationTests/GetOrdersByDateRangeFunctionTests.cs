using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using FluentAssertions;
using Order.Api.Features.CreateOrder;
using Order.Api.Features.GetOrdersByDateRange;
using Order.Api.Shared;
using Order.IntegrationTests.Fixtures;
using System.Text.Json;
using System.Text.Json.Serialization;
using Order.Api.Shared.Application.Dtos;
using Xunit;

namespace Order.IntegrationTests;

[Collection("OrderApi")]
public class GetOrdersByDateRangeFunctionTests(OrderApiFixture fixture)
{
    private readonly GetOrdersByDateRangeFunction _function = new(fixture.Mediator);
    private static readonly TestLambdaContext Context = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task Handler_WhenValidDateProvided_ShouldReturnOrdersForThatDay()
    {
        await CreateOrderAsync("cust-bydate-1");
        var today = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");

        var request = new APIGatewayProxyRequest
        {
            QueryStringParameters = new Dictionary<string, string> { ["date"] = today }
        };

        var response = await _function.Handler(request, Context);

        response.StatusCode.Should().Be(200);
        var body = JsonSerializer.Deserialize<ApiResponse<IEnumerable<OrderDto>>>(response.Body, JsonOptions);
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handler_WhenDateFormatIsInvalid_ShouldRejectRequest()
    {
        var request = new APIGatewayProxyRequest
        {
            QueryStringParameters = new Dictionary<string, string> { ["date"] = "not-a-date" }
        };

        var response = await _function.Handler(request, Context);

        response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Handler_WhenDateNotProvided_ShouldRejectRequest()
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
