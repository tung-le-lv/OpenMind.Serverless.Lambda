using Payment.Api.Domain.Events;

namespace Payment.Api.Shared.Application.Interfaces;

public interface IEventBus
{
    Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : IDomainEvent;
}
