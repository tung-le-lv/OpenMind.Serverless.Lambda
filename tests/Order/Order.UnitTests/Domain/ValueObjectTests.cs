using FluentAssertions;
using Order.Api.Domain.ValueObjects;
using Xunit;

namespace Order.UnitTests.Domain;

public class ValueObjectTests
{
    [Fact]
    public void Money_FromDecimal_ShouldCreateMoney()
    {
        var money = Money.FromDecimal(10.99m);

        money.Amount.Should().Be(10.99m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Money_Add_ShouldAddCorrectly()
    {
        var money1 = Money.FromDecimal(10.00m);
        var money2 = Money.FromDecimal(5.50m);

        var result = money1.Add(money2);

        result.Amount.Should().Be(15.50m);
    }

    [Fact]
    public void Money_Subtract_ShouldSubtractCorrectly()
    {
        var money1 = Money.FromDecimal(10.00m);
        var money2 = Money.FromDecimal(5.50m);

        var result = money1.Subtract(money2);

        result.Amount.Should().Be(4.50m);
    }

    [Fact]
    public void Money_Equality_ShouldWorkCorrectly()
    {
        var money1 = Money.FromDecimal(10.00m);
        var money2 = Money.FromDecimal(10.00m);

        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Address_Create_ShouldCreateAddress()
    {
        var address = Address.Create("123 Main St", "Seattle", "WA", "98101", "USA");

        address.Street.Should().Be("123 Main St");
        address.City.Should().Be("Seattle");
        address.State.Should().Be("WA");
        address.ZipCode.Should().Be("98101");
        address.Country.Should().Be("USA");
    }

    [Fact]
    public void Address_Create_WithEmptyStreet_ShouldThrowException()
    {
        var act = () => Address.Create("", "Seattle", "WA", "98101", "USA");

        act.Should().Throw<ArgumentException>()
            .WithMessage("Street is required.*");
    }

    [Fact]
    public void Address_Equality_ShouldWorkCorrectly()
    {
        var address1 = Address.Create("123 Main St", "Seattle", "WA", "98101", "USA");
        var address2 = Address.Create("123 Main St", "Seattle", "WA", "98101", "USA");

        address1.Should().Be(address2);
    }
}
