using FluentAssertions;
using Order.Domain.Entities;
using Xunit;

namespace Order.UnitTests.Domain;

public class OrderItemTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateItem()
    {
        // Act
        var item = OrderItem.Create("prod-1", "Product 1", 2, 10.00m);

        // Assert
        item.Should().NotBeNull();
        item.ProductId.Should().Be("prod-1");
        item.ProductName.Should().Be("Product 1");
        item.Quantity.Should().Be(2);
        item.UnitPrice.Amount.Should().Be(10.00m);
        item.Subtotal.Amount.Should().Be(20.00m);
    }

    [Fact]
    public void Create_WithEmptyProductId_ShouldThrowException()
    {
        // Act
        var act = () => OrderItem.Create("", "Product 1", 2, 10.00m);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Product ID is required.");
    }

    [Fact]
    public void Create_WithZeroQuantity_ShouldThrowException()
    {
        // Act
        var act = () => OrderItem.Create("prod-1", "Product 1", 0, 10.00m);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Quantity must be greater than zero.");
    }

    [Fact]
    public void Create_WithNegativePrice_ShouldThrowException()
    {
        // Act
        var act = () => OrderItem.Create("prod-1", "Product 1", 1, -10.00m);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Unit price cannot be negative.");
    }

    [Fact]
    public void IncreaseQuantity_ShouldIncreaseCorrectly()
    {
        // Arrange
        var item = OrderItem.Create("prod-1", "Product 1", 2, 10.00m);

        // Act
        item.IncreaseQuantity(3);

        // Assert
        item.Quantity.Should().Be(5);
        item.Subtotal.Amount.Should().Be(50.00m);
    }

    [Fact]
    public void DecreaseQuantity_ShouldDecreaseCorrectly()
    {
        // Arrange
        var item = OrderItem.Create("prod-1", "Product 1", 5, 10.00m);

        // Act
        item.DecreaseQuantity(3);

        // Assert
        item.Quantity.Should().Be(2);
    }

    [Fact]
    public void DecreaseQuantity_BelowOne_ShouldThrowException()
    {
        // Arrange
        var item = OrderItem.Create("prod-1", "Product 1", 2, 10.00m);

        // Act
        var act = () => item.DecreaseQuantity(3);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Quantity cannot be less than 1.");
    }
}
