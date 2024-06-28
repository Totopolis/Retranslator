using Domain.Entities.JsonRequest;

namespace Domain.Abstractions;

// TODO: split into two separated repos (read and write)
public interface IJsonRequestRepository
{
    Task<JsonRequest?> GetById(JsonRequestId id, CancellationToken ct = default!);

    void Insert(JsonRequest jsonRequest);
}
