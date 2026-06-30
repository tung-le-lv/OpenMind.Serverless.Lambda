using FluentAssertions;
using Moq;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Enums;
using Order.Api.Domain.Repositories;
using Order.Api.Domain.ValueObjects;
using Order.Api.Features.GetOrdersByCustomerAndStatus;
using Xunit;

namespace Order.UnitTests.Application;

public class GetOrdersByCustomerAndStatusQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _repo = new();
    private readonly GetOrdersByCustomerAndStatusQueryHandler _handler;

    public GetOrdersByCustomerAndStatusQueryHandlerTests() =>
        _handler = new GetOrdersByCustomerAndStatusQueryHandler(_repo.Object);

    [Fact]
    public async Task Handle_WhenFilteringByStatus_ShouldReturnMatchingOrders()
    {
        var orders = new List<OrderAggregate>
        {
            OrderAggregate.Reconstitute("o1", "cust-1", [OrderItem.Reconstitute("p1", "P1", 1, 10m)], 10m, OrderStatus.Confirmed, DateTime.UtcNow, DateTime.UtcNow)
        };
        _repo.Setup(r => r.GetByCustomerIdAndStatusAsync("cust-1", OrderStatus.Confirmed, It.IsAny<CancellationToken>())).ReturnsAsync(orders);

        var result = await _handler.Handle(new GetOrdersByCustomerAndStatusQuery("cust-1", OrderStatus.Confirmed), CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public async Task Handle_WhenNoOrdersMatchFilter_ShouldReturnEmptyList()
    {
        _repo.Setup(r => r.GetByCustomerIdAndStatusAsync(It.IsAny<string>(), It.IsAny<OrderStatus>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var result = await _handler.Handle(new GetOrdersByCustomerAndStatusQuery("cust-1", OrderStatus.Shipped), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
