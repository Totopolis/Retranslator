using Domain.Primitives;

namespace Application.Abstractions;

public interface IEventBus
{
    Task PublishAsync<T>(T message, CancellationToken ct = default)
        where T : class;

    Task PublishDelayedDomainEventAsync(IDomainEvent domainEvent, CancellationToken ct = default);
}
