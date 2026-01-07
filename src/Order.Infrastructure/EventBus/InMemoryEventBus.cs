using Order.Application.Interfaces;
using Order.Domain.Events;

namespace Order.Infrastructure.EventBus;

/// <summary>
/// In-memory event bus for local development and testing.
/// Events are logged but not persisted.
/// </summary>
public class InMemoryEventBus : IEventBus
{
    private readonly List<IDomainEvent> _events = [];

    public IReadOnlyList<IDomainEvent> PublishedEvents => _events.AsReadOnly();

    public Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : IDomainEvent
    {
        _events.Add(domainEvent);
        Console.WriteLine($"[EventBus] Published event: {domainEvent.EventType} (ID: {domainEvent.EventId})");
        return Task.CompletedTask;
    }

    public void Clear()
    {
        _events.Clear();
    }
}
