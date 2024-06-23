using Application.Abstractions;
using Domain.Entities.JsonRequest;
using Domain.Primitives;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Persistence;

public class PublishDomainEventsToEventBusInterceptor :
    SaveChangesInterceptor
{
    private readonly IEventBus _eventBus;

    public PublishDomainEventsToEventBusInterceptor(IEventBus eventBus)
    {
        _eventBus = eventBus;
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
            .Entries<AggregateRoot<JsonRequestId>>()
            .Select(x => x.Entity)
            .SelectMany(aggregateRoot =>
            {
                var domainEvents = aggregateRoot.GetDomainEvents();
                // TODO: preserve from events lost!!! error handling need
                aggregateRoot.ClearDomainEvents();
                return domainEvents;
            })
            .ToList();

        // TODO: use transactional outbox pattern!
        var taskList = events
            .Select(x => _eventBus.PublishAsync(x, cancellationToken))
            .ToArray();

        // DANGER: sync awaiting
        Task.WaitAll(taskList, cancellationToken);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
