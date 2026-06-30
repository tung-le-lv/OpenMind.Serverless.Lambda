using FluentAssertions;
using Moq;
using Order.Api.Application.Interfaces;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Enums;
using Order.Api.Domain.Events;
using Order.Api.Domain.Repositories;
using Order.Api.Features.CancelOrder;
using Xunit;

namespace Order.UnitTests.Application;

public class CancelOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _repo = new();
    private readonly Mock<IEventBus> _bus = new();
    private readonly CancelOrderCommandHandler _handler;

    public CancelOrderCommandHandlerTests() =>
        _handler = new CancelOrderCommandHandler(_repo.Object, _bus.Object);

    [Fact]
    public async Task Handle_WhenOrderIsPending_ShouldCancelSuccessfully()
    {
        var order = PendingOrder();
        _repo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<OrderAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _handler.Handle(new CancelOrderCommand(order.Id), CancellationToken.None);

        result.Success.Should().BeTrue();
        _repo.Verify(r => r.UpdateAsync(It.IsAny<OrderAggregate>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ShouldReturnOrderNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((OrderAggregate?)null);

        var result = await _handler.Handle(new CancelOrderCommand("missing"), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_WhenOrderIsShipped_ShouldNotAllowCancellation()
    {
        var order = ShippedOrder();
        _repo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _handler.Handle(new CancelOrderCommand(order.Id), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenItemAdded_ShouldPublishEvents()
    {
        var order = PendingOrder();
        _repo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<OrderAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(order);

        await _handler.Handle(new CancelOrderCommand(order.Id), CancellationToken.None);

        _bus.Verify(e => e.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    private static OrderAggregate PendingOrder() =>
        OrderAggregate.Reconstitute("order-1", "cust-1", [], 0m, OrderStatus.Pending, DateTime.UtcNow, DateTime.UtcNow);

    private static OrderAggregate ShippedOrder() =>
        OrderAggregate.Reconstitute("order-3", "cust-1", [], 0m, OrderStatus.Shipped, DateTime.UtcNow, DateTime.UtcNow);
}
