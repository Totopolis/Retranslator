using Domain.Abstractions;

namespace Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly RetranslatorDbContext _dbContext;

    public UnitOfWork(RetranslatorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // There can be - domain events to outbox or auditable entities
        return await _dbContext.SaveChangesAsync();
    }
}
