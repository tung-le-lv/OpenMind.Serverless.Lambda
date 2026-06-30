using FluentAssertions;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Enums;
using Order.Api.Domain.ValueObjects;
using Xunit;

namespace Order.UnitTests.Domain;

public class OrderAggregateTests
{
    [Fact]
    public void Create_WithValidCustomerId_ShouldCreateOrder()
    {
        var order = OrderAggregate.Create("customer-123");

        order.Should().NotBeNull();
        order.CustomerId.Should().Be("customer-123");
        order.Status.Should().Be(OrderStatus.Pending);
        order.Items.Should().BeEmpty();
        order.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ShouldThrowException()
    {
        var act = () => OrderAggregate.Create("");

        act.Should().Throw<DomainException>()
            .WithMessage("Customer ID is required.");
    }

    [Fact]
    public void Create_WithShippingAddress_ShouldSetAddress()
    {
        var address = Address.Create("123 Main St", "Seattle", "WA", "98101", "USA");

        var order = OrderAggregate.Create("customer-123", address);

        order.ShippingAddress.Should().NotBeNull();
        order.ShippingAddress!.City.Should().Be("Seattle");
    }

    [Fact]
    public void AddItem_ToPendingOrder_ShouldAddItem()
    {
        var order = OrderAggregate.Create("customer-123");

        order.AddItem("prod-1", "Product 1", 2, 10.00m);

        order.Items.Should().HaveCount(1);
        order.TotalAmount.Amount.Should().Be(20.00m);
    }

    [Fact]
    public void AddItem_SameProduct_ShouldIncreaseQuantity()
    {
        var order = OrderAggregate.Create("customer-123");
        order.AddItem("prod-1", "Product 1", 2, 10.00m);

        order.AddItem("prod-1", "Product 1", 3, 10.00m);

        order.Items.Should().HaveCount(1);
        order.Items.First().Quantity.Should().Be(5);
        order.TotalAmount.Amount.Should().Be(50.00m);
    }

    [Fact]
    public void UpdateStatus_ValidTransition_ShouldUpdateStatus()
    {
        var order = OrderAggregate.Create("customer-123");

        order.UpdateStatus(OrderStatus.Confirmed);

        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void UpdateStatus_InvalidTransition_ShouldThrowException()
    {
        var order = OrderAggregate.Create("customer-123");

        var act = () => order.UpdateStatus(OrderStatus.Shipped);

        act.Should().Throw<DomainException>()
            .WithMessage("Invalid status transition from Pending to Shipped.");
    }

    [Fact]
    public void Cancel_PendingOrder_ShouldCancelSuccessfully()
    {
        var order = OrderAggregate.Create("customer-123");

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ShippedOrder_ShouldThrowException()
    {
        var order = OrderAggregate.Create("customer-123");
        order.UpdateStatus(OrderStatus.Confirmed);
        order.UpdateStatus(OrderStatus.Processing);
        order.UpdateStatus(OrderStatus.Shipped);

        var act = () => order.Cancel();

        act.Should().Throw<DomainException>()
            .WithMessage("Cannot cancel an order that has been shipped or delivered.");
    }

    [Fact]
    public void RemoveItem_FromPendingOrder_ShouldRemoveItem()
    {
        var order = OrderAggregate.Create("customer-123");
        order.AddItem("prod-1", "Product 1", 2, 10.00m);
        order.AddItem("prod-2", "Product 2", 1, 15.00m);

        order.RemoveItem("prod-1");

        order.Items.Should().HaveCount(1);
        order.TotalAmount.Amount.Should().Be(15.00m);
    }
}
