using Domain.Abstractions;
using Domain.Entities.JsonRequest;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories;

// TODO: split repository into separated queries?
public class JsonRequestRepository : IJsonRequestRepository
{
    private readonly RetranslatorDbContext _dbContext;

    public JsonRequestRepository(RetranslatorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<JsonRequest?> GetById(JsonRequestId id, CancellationToken ct = default)
    {
        var finded = await _dbContext.Set<JsonRequest>()
            //.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    
        return finded;
    }

    public void Insert(JsonRequest jsonRequest)
    {
        _dbContext.Set<JsonRequest>().Add(jsonRequest);
    }
}
