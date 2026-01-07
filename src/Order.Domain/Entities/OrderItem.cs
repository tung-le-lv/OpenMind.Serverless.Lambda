using Order.Domain.ValueObjects;

namespace Order.Domain.Entities;

public class OrderItem
{
    public string ProductId { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = Money.Zero;
    public Money Subtotal => Money.FromDecimal(Quantity * UnitPrice.Amount);

    private OrderItem() { }

    public static OrderItem Create(string productId, string productName, int quantity, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new DomainException("Product ID is required.");

        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("Product name is required.");

        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        if (unitPrice < 0)
            throw new DomainException("Unit price cannot be negative.");

        return new OrderItem
        {
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = Money.FromDecimal(unitPrice)
        };
    }

    public static OrderItem Reconstitute(string productId, string productName, int quantity, decimal unitPrice)
    {
        return new OrderItem
        {
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = Money.FromDecimal(unitPrice)
        };
    }

    public void IncreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be greater than zero.");

        Quantity += amount;
    }

    public void DecreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be greater than zero.");

        if (Quantity - amount < 1)
            throw new DomainException("Quantity cannot be less than 1.");

        Quantity -= amount;
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        Quantity = newQuantity;
    }
}
