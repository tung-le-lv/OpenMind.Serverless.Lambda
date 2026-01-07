using FluentAssertions;
using Order.Domain.Entities;
using Order.Domain.Enums;
using Order.Domain.ValueObjects;
using Xunit;

namespace Order.UnitTests.Domain;

public class OrderEntityTests
{
    [Fact]
    public void Create_WithValidCustomerId_ShouldCreateOrder()
    {
        // Act
        var order = OrderEntity.Create("customer-123");

        // Assert
        order.Should().NotBeNull();
        order.CustomerId.Should().Be("customer-123");
        order.Status.Should().Be(OrderStatus.Pending);
        order.Items.Should().BeEmpty();
        order.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ShouldThrowException()
    {
        // Act
        var act = () => OrderEntity.Create("");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Customer ID is required.");
    }

    [Fact]
    public void Create_WithShippingAddress_ShouldSetAddress()
    {
        // Arrange
        var address = Address.Create("123 Main St", "Seattle", "WA", "98101", "USA");

        // Act
        var order = OrderEntity.Create("customer-123", address);

        // Assert
        order.ShippingAddress.Should().NotBeNull();
        order.ShippingAddress!.City.Should().Be("Seattle");
    }

    [Fact]
    public void AddItem_ToPendingOrder_ShouldAddItem()
    {
        // Arrange
        var order = OrderEntity.Create("customer-123");

        // Act
        order.AddItem("prod-1", "Product 1", 2, 10.00m);

        // Assert
        order.Items.Should().HaveCount(1);
        order.TotalAmount.Amount.Should().Be(20.00m);
    }

    [Fact]
    public void AddItem_SameProduct_ShouldIncreaseQuantity()
    {
        // Arrange
        var order = OrderEntity.Create("customer-123");
        order.AddItem("prod-1", "Product 1", 2, 10.00m);

        // Act
        order.AddItem("prod-1", "Product 1", 3, 10.00m);

        // Assert
        order.Items.Should().HaveCount(1);
        order.Items.First().Quantity.Should().Be(5);
        order.TotalAmount.Amount.Should().Be(50.00m);
    }

    [Fact]
    public void UpdateStatus_ValidTransition_ShouldUpdateStatus()
    {
        // Arrange
        var order = OrderEntity.Create("customer-123");

        // Act
        order.UpdateStatus(OrderStatus.Confirmed);

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void UpdateStatus_InvalidTransition_ShouldThrowException()
    {
        // Arrange
        var order = OrderEntity.Create("customer-123");

        // Act
        var act = () => order.UpdateStatus(OrderStatus.Shipped);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Invalid status transition from Pending to Shipped.");
    }

    [Fact]
    public void Cancel_PendingOrder_ShouldCancelSuccessfully()
    {
        // Arrange
        var order = OrderEntity.Create("customer-123");

        // Act
        order.Cancel();

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ShippedOrder_ShouldThrowException()
    {
        // Arrange
        var order = OrderEntity.Create("customer-123");
        order.UpdateStatus(OrderStatus.Confirmed);
        order.UpdateStatus(OrderStatus.Processing);
        order.UpdateStatus(OrderStatus.Shipped);

        // Act
        var act = () => order.Cancel();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Cannot cancel an order that has been shipped or delivered.");
    }

    [Fact]
    public void RemoveItem_FromPendingOrder_ShouldRemoveItem()
    {
        // Arrange
        var order = OrderEntity.Create("customer-123");
        order.AddItem("prod-1", "Product 1", 2, 10.00m);
        order.AddItem("prod-2", "Product 2", 1, 15.00m);

        // Act
        order.RemoveItem("prod-1");

        // Assert
        order.Items.Should().HaveCount(1);
        order.TotalAmount.Amount.Should().Be(15.00m);
    }
}
