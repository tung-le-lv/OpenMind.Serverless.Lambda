using Payment.Api.Domain.Events;

namespace Payment.Api.Application.Interfaces;

public interface IEventBus
{
    Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : IDomainEvent;
}
