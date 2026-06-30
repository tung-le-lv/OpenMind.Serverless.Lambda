namespace Payment.Api.Domain.Events;

public record PaymentFailedEvent(
    string PaymentId,
    string OrderId,
    string Reason
) : DomainEventBase
{
    public override string EventType => "PaymentFailed";
}
