using Application.Abstractions;
using Domain.Primitives;
using MassTransit;

namespace Persistence.MessageBroker;

public class EventBus : IEventBus
{
    private readonly IMessageScheduler _messageScheduler;
    private readonly IPublishEndpoint _publishEndpoint;

    public EventBus(
        IMessageScheduler messageScheduler,
        IPublishEndpoint publishEndpoint)
    {
        _messageScheduler = messageScheduler;
        _publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync<T>(T message, CancellationToken ct = default)
        where T : class
    {
        return _publishEndpoint.Publish<T>(message, ct);
    }

    public Task PublishDelayedDomainEventAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        var domainEventType = domainEvent.GetType();
        
        return _messageScheduler.SchedulePublish(
            // DANGER: DateTime.Now
            scheduledTime: DateTime.Now + TimeSpan.FromSeconds(1),
            messageType: domainEventType,
            message: domainEvent);
    }
}
