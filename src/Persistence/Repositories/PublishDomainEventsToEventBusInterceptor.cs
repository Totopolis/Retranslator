using Application.Abstractions;
using Domain.Entities.JsonRequest;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Persistence.Repositories;

public class PublishDomainEventsToEventBusInterceptor :
    SaveChangesInterceptor
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<PublishDomainEventsToEventBusInterceptor> _logger;

    public PublishDomainEventsToEventBusInterceptor(
        IEventBus eventBus,
        ILogger<PublishDomainEventsToEventBusInterceptor> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;

        if (dbContext is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var events = dbContext.ChangeTracker
            // TODO: need more generic approach
            .Entries<JsonRequest>()
            .Select(x => x.Entity)
            .SelectMany(aggregateRoot =>
            {
                var domainEvents = aggregateRoot.GetDomainEvents();
                // TODO: preserve from events lost!!! error handling need
                aggregateRoot.ClearDomainEvents();
                return domainEvents;
            })
            .ToList();

        if (events.Count == 0)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        // TODO: use transactional outbox pattern instead delayed send domain events
        var taskList = events
            .Select(x => _eventBus.PublishDelayedDomainEventAsync(x, cancellationToken))
            .ToArray();

        // DANGER: sync awaiting
        Task.WaitAll(taskList, cancellationToken);

        _logger.LogInformation("Domain events sended");

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
