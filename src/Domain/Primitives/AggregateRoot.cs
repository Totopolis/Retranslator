namespace Domain.Primitives;

public abstract class AggregateRoot<T> : Entity<T>
    where T : struct
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected AggregateRoot(T id) : base(id)
    {
    }

    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() =>
        _domainEvents.ToList();

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
