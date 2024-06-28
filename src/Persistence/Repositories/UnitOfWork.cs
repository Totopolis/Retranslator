using Application.Abstractions;
using Domain.Abstractions;
using Domain.Entities.JsonRequest;

namespace Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly IEventBus _eventBus;
    private readonly RetranslatorDbContext _dbContext;

    public UnitOfWork(
        IEventBus eventBus,
        RetranslatorDbContext dbContext)
    {
        _eventBus = eventBus;
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var count = await _dbContext.SaveChangesAsync(ct);

        await PublishDomainEvents(ct);
        
        return count;
    }

    // TODO: ConvertDomainEventsToOutboxMessages - use transactional outbox pattern
    // instead delayed send domain events
    private async Task PublishDomainEvents(CancellationToken ct)
    {
        var events = _dbContext.ChangeTracker
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
            return;
        }

        var taskList = events
            .Select(x => _eventBus.PublishDelayedDomainEventAsync(x, ct))
            .ToArray();

        await Task.WhenAll(taskList);

        // _logger.LogInformation("Domain events sended");
    }
}
