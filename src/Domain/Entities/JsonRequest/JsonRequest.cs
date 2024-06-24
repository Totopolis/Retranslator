using Domain.DomainEvents;
using Domain.Errors;
using Domain.Primitives;
using Domain.Shared;

namespace Domain.Entities.JsonRequest;

public sealed class JsonRequest : AggregateRoot<JsonRequestId>
{
    private JsonRequest(
        JsonRequestId id,
        string content,
        DateTime received,
        JsonRequestState state) : base(id)
    {
        Content = content;
        Received = received;
        State = state;
    }

    public string Content { get; private set; }

    public DateTime Received { get; private set; }

    public JsonRequestState State { get; private set; }

    public void Failed(string errorMessage)
    {
        State = JsonRequestState.Failed;

        RaiseDomainEvent(new JsonRequestFailedDomainEvent(
            Id,
            errorMessage));
    }

    public void Delivered()
    {
        State = JsonRequestState.Delivered;

        RaiseDomainEvent(new JsonRequestDeliveredDomainEvent(Id));
    }

    // U can provide some repo or service for incupsulate logic (checks for exmpl)
    public static Result<JsonRequest> CreateNew(string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            return Result.Failure<JsonRequest>(DomainErrors.JsonRequest.CreateNew);
        }

        var id = new JsonRequestId(Guid.NewGuid());
        var msg = new JsonRequest(
            id,
            jsonContent,
            DateTime.UtcNow,
            JsonRequestState.Received);

        msg.RaiseDomainEvent(new JsonRequestReceivedDomainEvent(
            id,
            jsonContent,
            msg.Received));

        return msg;
    }
}
