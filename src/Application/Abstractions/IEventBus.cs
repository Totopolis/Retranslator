namespace Application.Abstractions;

public interface IEventBus
{
    Task PublishAsync<T>(T message, CancellationToken ct = default)
        where T : class;

    Task PublishAsync(object message, CancellationToken ct = default);
}
