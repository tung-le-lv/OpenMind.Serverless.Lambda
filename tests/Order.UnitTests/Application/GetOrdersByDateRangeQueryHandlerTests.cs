using FluentAssertions;
using Moq;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Enums;
using Order.Api.Domain.Repositories;
using Order.Api.Domain.ValueObjects;
using Order.Api.Features.GetOrdersByDateRange;
using Xunit;

namespace Order.UnitTests.Application;

public class GetOrdersByDateRangeQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _repo = new();
    private readonly GetOrdersByDateRangeHandler _handler;

    public GetOrdersByDateRangeQueryHandlerTests() =>
        _handler = new GetOrdersByDateRangeHandler(_repo.Object);

    [Fact]
    public async Task Handle_WhenQueryingByDate_ShouldReturnOrdersForThatDay()
    {
        var date = new DateOnly(2024, 6, 1);
        var orders = new List<OrderAggregate>
        {
            OrderAggregate.Reconstitute("o1", "cust-1", [OrderItem.Reconstitute("p1", "P1", 1, 10m)], 10m, OrderStatus.Pending, null, DateTime.UtcNow, DateTime.UtcNow)
        };
        _repo.Setup(r => r.GetByDateAsync(date, It.IsAny<CancellationToken>())).ReturnsAsync(orders);

        var result = await _handler.Handle(new GetOrdersByDateRangeQuery(date), CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WhenNoOrdersExist_ShouldReturnEmptyList()
    {
        _repo.Setup(r => r.GetByDateAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var result = await _handler.Handle(new GetOrdersByDateRangeQuery(new DateOnly(2020, 1, 1)), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
