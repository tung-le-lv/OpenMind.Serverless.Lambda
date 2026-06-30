namespace Order.Api.Domain.Events;

public record OrderPlacedEvent(
    string OrderId,
    string CustomerId,
    decimal TotalAmount
) : DomainEventBase
{
    public override string EventType => "OrderPlaced";
}
