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
}
