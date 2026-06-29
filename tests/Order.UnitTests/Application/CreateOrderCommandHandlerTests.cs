using FluentAssertions;
using Moq;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Events;
using Order.Api.Application.Interfaces;
using Order.Api.Domain.Repositories;
using Order.Api.Features.CreateOrder;
using Order.Api.Application.Dtos;
using Xunit;

namespace Order.UnitTests.Application;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _mockRepository;
    private readonly Mock<IEventBus> _mockEventBus;
    private readonly CreateOrderHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _mockRepository = new Mock<IOrderRepository>();
        _mockEventBus = new Mock<IEventBus>();
        _handler = new CreateOrderHandler(_mockRepository.Object, _mockEventBus.Object);
    }

    [Fact]
    public async Task Handle_WhenValidOrderSubmitted_ShouldCreateOrder()
    {
        var command = new CreateOrderCommand(
            CustomerId: "customer-123",
            Items: [new CreateOrderItemDto("prod-1", "Product 1", 2, 10.00m)],
            ShippingAddress: new AddressDto("123 Main St", "Seattle", "WA", "98101", "USA")
        );

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<OrderAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderAggregate o, CancellationToken _) => o);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.OrderId.Should().NotBeNullOrEmpty();
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<OrderAggregate>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCustomerIdMissing_ShouldRejectOrder()
    {
        var command = new CreateOrderCommand(
            CustomerId: "",
            Items: [new CreateOrderItemDto("prod-1", "Product 1", 2, 10.00m)],
            ShippingAddress: null
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenItemAdded_ShouldPublishEvents()
    {
        var command = new CreateOrderCommand(
            CustomerId: "customer-123",
            Items: [new CreateOrderItemDto("prod-1", "Product 1", 2, 10.00m)],
            ShippingAddress: null
        );

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<OrderAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderAggregate o, CancellationToken _) => o);

        await _handler.Handle(command, CancellationToken.None);

        _mockEventBus.Verify(e => e.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}
