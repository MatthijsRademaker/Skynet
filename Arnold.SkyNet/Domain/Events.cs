namespace Arnold.SkyNet.Domain;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
    Guid AggregateId { get; }
    int Version { get; set; }
}

public abstract class DomainEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public abstract Guid AggregateId { get; }
    public int Version { get; set; }
}

// Events specific to your domain
public class CustomerCreatedEvent(Guid customerId, string name, string email) : DomainEvent
{
    public override Guid AggregateId => customerId;

    public string Name { get; } = name;
    public string Email { get; } = email;
}

public class PremiumCalculatedEvent(Guid customerId, decimal amount) : DomainEvent
{
    public readonly decimal Amount = amount;

    public override Guid AggregateId => customerId;
}

public interface IEventStore
{
    Task SaveEvents(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken);
    Task<IReadOnlyList<IDomainEvent>> GetEvents(
        Guid aggregateId,
        CancellationToken cancellationToken
    );
}
