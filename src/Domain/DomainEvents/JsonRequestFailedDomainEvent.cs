using Domain.Entities.JsonRequest;
using Domain.Primitives;

namespace Domain.DomainEvents;

public sealed record JsonRequestFailedDomainEvent(
    JsonRequestId Id,
    string ErrorMessage) : IDomainEvent;
