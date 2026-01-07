using FluentAssertions;
using Moq;
using Order.Application.Commands;
using Order.Application.Handlers.Commands;
using Order.Application.Interfaces;
using Order.Domain.Entities;
using Order.Domain.Events;
using Order.Domain.Repositories;
using Xunit;

namespace Order.UnitTests.Application;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _mockRepository;
    private readonly Mock<IEventBus> _mockEventBus;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _mockRepository = new Mock<IOrderRepository>();
        _mockEventBus = new Mock<IEventBus>();
        _handler = new CreateOrderCommandHandler(_mockRepository.Object, _mockEventBus.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateOrder()
    {
        // Arrange
        var command = new CreateOrderCommand(
            CustomerId: "customer-123",
            Items:
            [
                new CreateOrderItemDto("prod-1", "Product 1", 2, 10.00m)
            ],
            ShippingAddress: new AddressDto("123 Main St", "Seattle", "WA", "98101", "USA")
        );

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderEntity o, CancellationToken _) => o);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.OrderId.Should().NotBeNullOrEmpty();
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyCustomerId_ShouldReturnError()
    {
        // Arrange
        var command = new CreateOrderCommand(
            CustomerId: "",
            Items:
            [
                new CreateOrderItemDto("prod-1", "Product 1", 2, 10.00m)
            ],
            ShippingAddress: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldPublishDomainEvents()
    {
        // Arrange
        var command = new CreateOrderCommand(
            CustomerId: "customer-123",
            Items:
            [
                new CreateOrderItemDto("prod-1", "Product 1", 2, 10.00m)
            ],
            ShippingAddress: null
        );

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderEntity o, CancellationToken _) => o);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockEventBus.Verify(e => e.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}
