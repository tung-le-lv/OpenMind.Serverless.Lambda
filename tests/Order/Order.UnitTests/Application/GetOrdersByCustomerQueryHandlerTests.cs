using FluentAssertions;
using Moq;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Enums;
using Order.Api.Domain.Repositories;
using Order.Api.Domain.ValueObjects;
using Order.Api.Features.GetOrdersByCustomer;
using Xunit;

namespace Order.UnitTests.Application;

public class GetOrdersByCustomerQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _repo = new();
    private readonly GetOrdersByCustomerQueryHandler _handler;

    public GetOrdersByCustomerQueryHandlerTests() =>
        _handler = new GetOrdersByCustomerQueryHandler(_repo.Object);

    [Fact]
    public async Task Handle_WhenCustomerHasOrders_ShouldReturnTheirOrders()
    {
        var orders = new List<OrderAggregate>
        {
            MakeOrder("o1", "cust-1", OrderStatus.Pending),
            MakeOrder("o2", "cust-1", OrderStatus.Confirmed)
        };
        _repo.Setup(r => r.GetByCustomerIdAsync("cust-1", It.IsAny<CancellationToken>())).ReturnsAsync(orders);

        var result = await _handler.Handle(new GetOrdersByCustomerQuery("cust-1"), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(o => o.CustomerId.Should().Be("cust-1"));
    }

    [Fact]
    public async Task Handle_WhenNoOrdersExist_ShouldReturnEmptyList()
    {
        _repo.Setup(r => r.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var result = await _handler.Handle(new GetOrdersByCustomerQuery("no-orders"), CancellationToken.None);

        result.Should().BeEmpty();
    }

    private static OrderAggregate MakeOrder(string id, string customerId, OrderStatus status) =>
        OrderAggregate.Reconstitute(id, customerId, [OrderItem.Reconstitute("p1", "P1", 1, 10m)], 10m, status, DateTime.UtcNow, DateTime.UtcNow);
}
