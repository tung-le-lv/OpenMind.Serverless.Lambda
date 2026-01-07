using FluentAssertions;
using Moq;
using Order.Application.DTOs;
using Order.Application.Handlers.Queries;
using Order.Application.Queries;
using Order.Domain.Entities;
using Order.Domain.Enums;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;
using Xunit;

namespace Order.UnitTests.Application;

public class GetOrderQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _mockRepository;
    private readonly GetOrderQueryHandler _handler;

    public GetOrderQueryHandlerTests()
    {
        _mockRepository = new Mock<IOrderRepository>();
        _handler = new GetOrderQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_WithExistingOrder_ShouldReturnOrderDto()
    {
        // Arrange
        var orderId = "order-123";
        var order = CreateTestOrder(orderId, "customer-123");

        _mockRepository.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var query = new GetOrderQuery(orderId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(orderId);
        result.CustomerId.Should().Be("customer-123");
    }

    [Fact]
    public async Task Handle_WithNonExistingOrder_ShouldReturnNull()
    {
        // Arrange
        var orderId = "non-existent";
        _mockRepository.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderEntity?)null);

        var query = new GetOrderQuery(orderId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    private static OrderEntity CreateTestOrder(string orderId, string customerId)
    {
        return OrderEntity.Reconstitute(
            id: orderId,
            customerId: customerId,
            items: [OrderItem.Reconstitute("prod-1", "Product 1", 2, 10.00m)],
            totalAmount: 20.00m,
            status: OrderStatus.Pending,
            shippingAddress: Address.Create("123 Main St", "Seattle", "WA", "98101", "USA"),
            createdAt: DateTime.UtcNow,
            updatedAt: DateTime.UtcNow
        );
    }
}
