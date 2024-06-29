using Application.Abstractions;
using Domain.Primitives;
using MassTransit;

namespace Persistence.MessageBroker;

public class EventBus : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;

    public EventBus(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync<T>(T message, CancellationToken ct = default)
        where T : class
    {
        return _publishEndpoint.Publish<T>(message, ct);
    }

    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        var domainEventType = domainEvent.GetType();

        return _publishEndpoint.Publish(
            message: domainEvent,
            messageType: domainEventType);
    }
}
