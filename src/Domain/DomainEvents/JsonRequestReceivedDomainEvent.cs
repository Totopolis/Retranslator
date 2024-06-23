using Domain.Entities.JsonRequest;
using Domain.Primitives;

namespace Domain.DomainEvents;

public sealed record JsonRequestReceivedDomainEvent(
    JsonRequestId Id,
    string Content,
    DateTime Received) : IDomainEvent;
