using Application.Abstractions;
using Domain.Abstractions;
using Domain.Entities.JsonRequest;
using Microsoft.Extensions.Logging;

namespace Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly IEventBus _eventBus;
    private readonly RetranslatorDbContext _dbContext;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(
        IEventBus eventBus,
        RetranslatorDbContext dbContext,
        ILogger<UnitOfWork> logger)
    {
        _eventBus = eventBus;
        _dbContext = dbContext;
        _logger = logger;
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
            .Select(x => _eventBus.PublishAsync(x, ct))
            .ToArray();

        await Task.WhenAll(taskList);

        _logger.LogInformation($"Domain events ({events.Count}) sended");
    }
}
