using FluentAssertions;
using Moq;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Enums;
using Order.Api.Domain.Repositories;
using Order.Api.Domain.ValueObjects;
using Order.Api.Features.GetOrder;
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
    public async Task Handle_WhenOrderExists_ShouldReturnOrderDetails()
    {
        var orderId = "order-123";
        var order = CreateTestOrder(orderId, "customer-123");

        _mockRepository.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(new GetOrderQuery(orderId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(orderId);
        result.CustomerId.Should().Be("customer-123");
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ShouldReturnNothing()
    {
        var orderId = "non-existent";
        _mockRepository.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderAggregate?)null);

        var result = await _handler.Handle(new GetOrderQuery(orderId), CancellationToken.None);

        result.Should().BeNull();
    }

    private static OrderAggregate CreateTestOrder(string orderId, string customerId) =>
        OrderAggregate.Reconstitute(
            id: orderId,
            customerId: customerId,
            items: [OrderItem.Reconstitute("prod-1", "Product 1", 2, 10.00m)],
            totalAmount: 20.00m,
            status: OrderStatus.Pending,
            createdAt: DateTime.UtcNow,
            updatedAt: DateTime.UtcNow
        );
}
