using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using FluentAssertions;
using Order.Api.Features.CreateOrder;
using Order.Api.Shared;
using Order.IntegrationTests.Fixtures;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Order.IntegrationTests;

[Collection("OrderApi")]
public class CreateOrderFunctionTests(OrderApiFixture fixture)
{
    private readonly CreateOrderFunction _function = new(fixture.Mediator);
    private static readonly TestLambdaContext Context = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task Handler_WhenValidOrderSubmitted_ShouldCreateOrderAndReturnId()
    {
        var request = new APIGatewayProxyRequest
        {
            Body = JsonSerializer.Serialize(new
            {
                customerId = "cust-create-1",
                items = new[] { new { productId = "p1", productName = "Widget", quantity = 2, unitPrice = 15.00m } }
            }, JsonOptions)
        };

        var response = await _function.Handler(request, Context);

        response.StatusCode.Should().Be(201);
        var body = JsonSerializer.Deserialize<ApiResponse<JsonElement>>(response.Body, JsonOptions);
        body!.Success.Should().BeTrue();
        body.Data.GetProperty("orderId").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handler_WhenNoRequestBodyProvided_ShouldRejectOrder()
    {
        var response = await _function.Handler(new APIGatewayProxyRequest { Body = null }, Context);

        response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Handler_WhenCustomerIdMissing_ShouldRejectOrder()
    {
        var request = new APIGatewayProxyRequest
        {
            Body = JsonSerializer.Serialize(new
            {
                customerId = "",
                items = new[] { new { productId = "p1", productName = "Widget", quantity = 1, unitPrice = 10m } }
            }, JsonOptions)
        };

        var response = await _function.Handler(request, Context);

        response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Handler_WhenNoItemsProvided_ShouldRejectOrder()
    {
        var request = new APIGatewayProxyRequest
        {
            Body = JsonSerializer.Serialize(new
            {
                customerId = "cust-create-2",
                items = Array.Empty<object>()
            }, JsonOptions)
        };

        var response = await _function.Handler(request, Context);

        response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Handler_WhenOrderIncludesShippingAddress_ShouldCreateOrderSuccessfully()
    {
        var request = new APIGatewayProxyRequest
        {
            Body = JsonSerializer.Serialize(new
            {
                customerId = "cust-create-3",
                items = new[] { new { productId = "p1", productName = "Widget", quantity = 1, unitPrice = 20m } },
                shippingAddress = new { street = "1 Main St", city = "Seattle", state = "WA", zipCode = "98101", country = "USA" }
            }, JsonOptions)
        };

        var response = await _function.Handler(request, Context);

        response.StatusCode.Should().Be(201);
    }
}
