using Domain.Abstractions;
using Domain.Entities.JsonRequest;

namespace Persistence.Repositories;

public class JsonRequestRepository : IJsonRequestRepository
{
    private readonly RetranslatorDbContext _dbContext;

    public JsonRequestRepository(RetranslatorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<JsonRequest?> GetById(JsonRequestId id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public void Insert(JsonRequest jsonRequest)
    {
        _dbContext.Set<JsonRequest>().Add(jsonRequest);
    }
}
