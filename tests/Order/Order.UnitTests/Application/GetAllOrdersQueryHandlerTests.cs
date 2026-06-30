using FluentAssertions;
using Moq;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Enums;
using Order.Api.Domain.Repositories;
using Order.Api.Domain.ValueObjects;
using Order.Api.Features.GetAllOrders;
using Xunit;

namespace Order.UnitTests.Application;

public class GetAllOrdersQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _repo = new();
    private readonly GetAllOrdersQueryHandler _handler;

    public GetAllOrdersQueryHandlerTests() =>
        _handler = new GetAllOrdersQueryHandler(_repo.Object);

    [Fact]
    public async Task Handle_ShouldReturnAllOrders()
    {
        var orders = new List<OrderAggregate>
        {
            MakeOrder("o1", "c1"),
            MakeOrder("o2", "c2")
        };
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(orders);

        var result = await _handler.Handle(new GetAllOrdersQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(o => o.Id).Should().BeEquivalentTo(["o1", "o2"]);
    }

    [Fact]
    public async Task Handle_WhenNoOrdersExist_ShouldReturnEmptyList()
    {
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var result = await _handler.Handle(new GetAllOrdersQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    private static OrderAggregate MakeOrder(string id, string customerId) =>
        OrderAggregate.Reconstitute(id, customerId, [OrderItem.Reconstitute("p1", "P1", 1, 10m)], 10m, OrderStatus.Pending, DateTime.UtcNow, DateTime.UtcNow);
}
