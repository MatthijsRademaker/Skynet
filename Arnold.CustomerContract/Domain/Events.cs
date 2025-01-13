namespace Arnold.CustomerContract.Events;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
    Guid AggregateId { get; init; }
    int Version { get; set; }
}

public abstract class DomainEvent : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public abstract Guid AggregateId { get; init; }
    public int Version { get; set; }
}

// Events specific to your domain
public class CustomerCreatedEvent : DomainEvent
{
    public CustomerCreatedEvent(Guid aggregateId, string name, string email)
    {
        AggregateId = aggregateId;
        Name = name;
        Email = email;
    }

    public override Guid AggregateId { get; init; }
    public string Name { get; init; }
    public string Email { get; init; }
}

public class PremiumCalculatedEvent(Guid customerId, decimal amount) : DomainEvent
{
    public readonly decimal Amount = amount;

    public override Guid AggregateId { get; init; } = customerId;
}

public interface IEventStore
{
    Task SaveEvents(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken);
    Task<IReadOnlyList<IDomainEvent>> GetEvents(
        Guid aggregateId,
        CancellationToken cancellationToken
    );
}
