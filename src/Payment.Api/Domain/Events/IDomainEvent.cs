namespace Payment.Api.Domain.Events;

public interface IDomainEvent
{
    string EventId { get; }
    DateTime OccurredAt { get; }
    string EventType { get; }
    string MessageGroupId { get; }
}

public abstract record DomainEventBase : IDomainEvent
{
    public string EventId { get; } = Guid.NewGuid().ToString();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public abstract string EventType { get; }
    public virtual string MessageGroupId => EventType;
}
