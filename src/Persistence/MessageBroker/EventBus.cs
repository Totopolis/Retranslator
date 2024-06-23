using Application.Abstractions;
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

    public Task PublishAsync(object message, CancellationToken ct = default)
    {
        var messageType = message.GetType();
        return _publishEndpoint.Publish(message, messageType, ct);
    }
}
