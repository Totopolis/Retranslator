using Domain.Entities.JsonRequest;

namespace Domain.Abstractions;

public interface IJsonRequestRepository
{
    Task<JsonRequest?> GetById(JsonRequestId id, CancellationToken ct = default!);

    void Insert(JsonRequest jsonRequest);
}
