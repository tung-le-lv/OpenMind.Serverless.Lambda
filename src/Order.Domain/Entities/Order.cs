using Order.Domain.Enums;
using Order.Domain.Events;
using Order.Domain.ValueObjects;

namespace Order.Domain.Entities;

public class OrderEntity
{
    private readonly List<OrderItem> _items = [];
    private readonly List<IDomainEvent> _domainEvents = [];

    public string Id { get; private set; } = string.Empty;
    public string CustomerId { get; private set; } = string.Empty;
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public Money TotalAmount { get; private set; } = Money.Zero;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public Address? ShippingAddress { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private OrderEntity() { }

    public static OrderEntity Create(string customerId, Address? shippingAddress = null)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new DomainException("Customer ID is required.");

        var order = new OrderEntity
        {
            Id = Guid.NewGuid().ToString(),
            CustomerId = customerId,
            ShippingAddress = shippingAddress,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        order.AddDomainEvent(new OrderCreatedEvent(order.Id, customerId));
        return order;
    }

    public static OrderEntity Reconstitute(
        string id,
        string customerId,
        List<OrderItem> items,
        decimal totalAmount,
        OrderStatus status,
        Address? shippingAddress,
        DateTime createdAt,
        DateTime updatedAt)
    {
        return new OrderEntity
        {
            Id = id,
            CustomerId = customerId,
            _items = { },
            TotalAmount = Money.FromDecimal(totalAmount),
            Status = status,
            ShippingAddress = shippingAddress,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        }.WithItems(items);
    }

    private OrderEntity WithItems(List<OrderItem> items)
    {
        _items.Clear();
        _items.AddRange(items);
        return this;
    }

    public void AddItem(string productId, string productName, int quantity, decimal unitPrice)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Cannot add items to a non-pending order.");

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            var item = OrderItem.Create(productId, productName, quantity, unitPrice);
            _items.Add(item);
        }

        RecalculateTotal();
        UpdateTimestamp();
        AddDomainEvent(new OrderItemAddedEvent(Id, productId, quantity));
    }

    public void RemoveItem(string productId)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Cannot remove items from a non-pending order.");

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new DomainException($"Item with product ID '{productId}' not found.");

        _items.Remove(item);
        RecalculateTotal();
        UpdateTimestamp();
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        ValidateStatusTransition(newStatus);
        var oldStatus = Status;
        Status = newStatus;
        UpdateTimestamp();
        AddDomainEvent(new OrderStatusChangedEvent(Id, oldStatus, newStatus));
    }

    public void Cancel()
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered)
            throw new DomainException("Cannot cancel an order that has been shipped or delivered.");

        var oldStatus = Status;
        Status = OrderStatus.Cancelled;
        UpdateTimestamp();
        AddDomainEvent(new OrderCancelledEvent(Id, oldStatus));
    }

    public void UpdateShippingAddress(Address address)
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered)
            throw new DomainException("Cannot update shipping address for shipped or delivered orders.");

        ShippingAddress = address;
        UpdateTimestamp();
    }

    private void ValidateStatusTransition(OrderStatus newStatus)
    {
        var validTransitions = new Dictionary<OrderStatus, OrderStatus[]>
        {
            { OrderStatus.Pending, [OrderStatus.Confirmed, OrderStatus.Cancelled] },
            { OrderStatus.Confirmed, [OrderStatus.Processing, OrderStatus.Cancelled] },
            { OrderStatus.Processing, [OrderStatus.Shipped, OrderStatus.Cancelled] },
            { OrderStatus.Shipped, [OrderStatus.Delivered] },
            { OrderStatus.Delivered, [] },
            { OrderStatus.Cancelled, [] }
        };

        if (!validTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
        {
            throw new DomainException($"Invalid status transition from {Status} to {newStatus}.");
        }
    }

    private void RecalculateTotal()
    {
        var total = _items.Sum(item => item.Subtotal.Amount);
        TotalAmount = Money.FromDecimal(total);
    }

    private void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
