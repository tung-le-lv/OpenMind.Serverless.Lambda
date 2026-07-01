namespace Order.Api.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5,
    PaymentConfirmed = 6
}

public static class OrderStatusExtensions
{
    private static readonly Dictionary<OrderStatus, OrderStatus[]> ValidTransitions = new()
    {
        { OrderStatus.Pending, [OrderStatus.Confirmed, OrderStatus.Cancelled, OrderStatus.PaymentConfirmed] },
        { OrderStatus.PaymentConfirmed, [OrderStatus.Processing, OrderStatus.Cancelled] },
        { OrderStatus.Confirmed, [OrderStatus.Processing, OrderStatus.Cancelled, OrderStatus.PaymentConfirmed] },
        { OrderStatus.Processing, [OrderStatus.Shipped, OrderStatus.Cancelled] },
        { OrderStatus.Shipped, [OrderStatus.Delivered] },
        { OrderStatus.Delivered, [] },
        { OrderStatus.Cancelled, [] }
    };

    public static bool CanTransitionTo(this OrderStatus status, OrderStatus newStatus) =>
        ValidTransitions.TryGetValue(status, out var allowed) && allowed.Contains(newStatus);
}
