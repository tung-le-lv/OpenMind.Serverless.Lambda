namespace Order.Domain.ValueObjects;

public sealed class Money : IEquatable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    public static Money Zero => new(0, "USD");

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money FromDecimal(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));

        return new Money(Math.Round(amount, 2), currency);
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        var result = Amount - other.Amount;
        if (result < 0)
            throw new InvalidOperationException("Result cannot be negative.");

        return new Money(result, Currency);
    }

    public Money Multiply(int factor)
    {
        if (factor < 0)
            throw new ArgumentException("Factor cannot be negative.", nameof(factor));

        return new Money(Amount * factor, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot operate on different currencies: {Currency} and {other.Currency}");
    }

    public bool Equals(Money? other)
    {
        if (other is null) return false;
        return Amount == other.Amount && Currency == other.Currency;
    }

    public override bool Equals(object? obj) => Equals(obj as Money);

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);

    public static bool operator ==(Money? left, Money? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(Money? left, Money? right) => !(left == right);

    public override string ToString() => $"{Currency} {Amount:F2}";
}
