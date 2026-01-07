namespace Order.Domain.Events;

public interface IDomainEvent
{
    string EventId { get; }
    DateTime OccurredAt { get; }
    string EventType { get; }
}

public abstract class DomainEventBase : IDomainEvent
{
    public string EventId { get; } = Guid.NewGuid().ToString();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}
