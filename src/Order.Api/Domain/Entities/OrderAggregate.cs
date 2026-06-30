using Order.Api.Domain.Enums;
using Order.Api.Domain.Events;
using Order.Api.Domain.ValueObjects;

namespace Order.Api.Domain.Entities;

public class OrderAggregate
{
    private readonly List<OrderItem> _items = [];
    private readonly List<IDomainEvent> _domainEvents = [];

    public string Id { get; private init; } = string.Empty;
    public string CustomerId { get; private set; } = string.Empty;
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public Money TotalAmount { get; private set; } = Money.Zero;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static OrderAggregate Create(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new DomainException("Customer ID is required.");
        }

        var order = new OrderAggregate
        {
            Id = Guid.NewGuid().ToString(),
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        order.AddDomainEvent(new OrderCreatedEvent(order.Id, customerId));
        return order;
    }

    public void Place()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new DomainException("Only a pending order can be placed.");
        }

        if (!_items.Any())
        {
            throw new DomainException("Cannot place an empty order.");
        }

        Status = OrderStatus.Confirmed;
        UpdateTimestamp();
        AddDomainEvent(new OrderPlacedEvent(Id, CustomerId, TotalAmount.Amount));
    }

    public static OrderAggregate Reconstitute(
        string id,
        string customerId,
        List<OrderItem> items,
        decimal totalAmount,
        OrderStatus status,
        DateTime createdAt,
        DateTime updatedAt)
    {
        return new OrderAggregate
        {
            Id = id,
            CustomerId = customerId,
            _items =
            {
                Capacity = 0
            },
            TotalAmount = Money.FromDecimal(totalAmount),
            Status = status,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        }.WithItems(items);
    }

    public void AddItem(string productId, string productName, int quantity, decimal unitPrice)
    {
        if (Status != OrderStatus.Pending)
        {
            throw new DomainException("Cannot add items to a non-pending order.");
        }

        var index = _items.FindIndex(i => i.ProductId == productId);
        if (index >= 0)
        {
            _items[index] = _items[index].IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(OrderItem.Create(productId, productName, quantity, unitPrice));
        }

        RecalculateTotal();
        UpdateTimestamp();
        AddDomainEvent(new OrderItemAddedEvent(Id, productId, quantity));
    }

    public void RemoveItem(string productId)
    {
        if (Status != OrderStatus.Pending)
        {
            throw new DomainException("Cannot remove items from a non-pending order.");
        }

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
        {
            throw new DomainException($"Item with product ID '{productId}' not found.");
        }

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
        {
            throw new DomainException("Cannot cancel an order that has been shipped or delivered.");
        }

        var oldStatus = Status;
        Status = OrderStatus.Cancelled;
        UpdateTimestamp();
        AddDomainEvent(new OrderCancelledEvent(Id, oldStatus));
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private OrderAggregate WithItems(List<OrderItem> items)
    {
        _items.Clear();
        _items.AddRange(items);
        return this;
    }

    private void ValidateStatusTransition(OrderStatus newStatus)
    {
        var validTransitions = new Dictionary<OrderStatus, OrderStatus[]>
        {
            { OrderStatus.Pending, [OrderStatus.Confirmed, OrderStatus.Cancelled, OrderStatus.PaymentConfirmed] },
            { OrderStatus.PaymentConfirmed, [OrderStatus.Processing, OrderStatus.Cancelled] },
            { OrderStatus.Confirmed, [OrderStatus.Processing, OrderStatus.Cancelled, OrderStatus.PaymentConfirmed] },
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
}
