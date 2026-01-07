using Order.Domain.Enums;

namespace Order.Domain.Events;

public class OrderCreatedEvent : DomainEventBase
{
    public override string EventType => "OrderCreated";
    public string OrderId { get; }
    public string CustomerId { get; }

    public OrderCreatedEvent(string orderId, string customerId)
    {
        OrderId = orderId;
        CustomerId = customerId;
    }
}

public class OrderItemAddedEvent : DomainEventBase
{
    public override string EventType => "OrderItemAdded";
    public string OrderId { get; }
    public string ProductId { get; }
    public int Quantity { get; }

    public OrderItemAddedEvent(string orderId, string productId, int quantity)
    {
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
    }
}

public class OrderStatusChangedEvent : DomainEventBase
{
    public override string EventType => "OrderStatusChanged";
    public string OrderId { get; }
    public OrderStatus OldStatus { get; }
    public OrderStatus NewStatus { get; }

    public OrderStatusChangedEvent(string orderId, OrderStatus oldStatus, OrderStatus newStatus)
    {
        OrderId = orderId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}

public class OrderCancelledEvent : DomainEventBase
{
    public override string EventType => "OrderCancelled";
    public string OrderId { get; }
    public OrderStatus PreviousStatus { get; }

    public OrderCancelledEvent(string orderId, OrderStatus previousStatus)
    {
        OrderId = orderId;
        PreviousStatus = previousStatus;
    }
}
