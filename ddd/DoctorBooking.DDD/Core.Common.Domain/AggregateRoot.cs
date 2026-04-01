namespace Core.Common.Domain;

public abstract class AggregateRoot<TId> : Entity<TId>
{
    private readonly List<DomainEvent> _domainEvents = [];

    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot(TId id) : base(id) { }

    protected AggregateRoot() { } // For ORM hydration

    protected void RegisterEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public IReadOnlyList<DomainEvent> PopDomainEvents()
    {
        var events = _domainEvents.ToList();
        _domainEvents.Clear();
        return events;
    }
}
