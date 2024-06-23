using Domain.Entities.JsonRequest;
using Domain.Primitives;

namespace Domain.DomainEvents;

public sealed record JsonRequestDeliveredDomainEvent(
    JsonRequestId Id) : IDomainEvent;
