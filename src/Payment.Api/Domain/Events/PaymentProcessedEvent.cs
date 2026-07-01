namespace Payment.Api.Domain.Events;

public record PaymentProcessedEvent(
    string PaymentId,
    string OrderId,
    string CustomerId,
    decimal Amount
) : DomainEventBase
{
    public override string EventType => "PaymentProcessed";
    public override string MessageGroupId => OrderId;
}
