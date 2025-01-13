using System.Text.Json.Serialization;

namespace Arnold.CustomerContract.Events;

public interface IDomainEvent
{
    Guid Id { get; init; }
    DateTime OccurredOn { get; init; }
    Guid AggregateId { get; init; }
    int Version { get; init; }
}

public abstract record DomainEvent : IDomainEvent
{
    [JsonConstructor]
    public DomainEvent(Guid aggregateId, int version)
    {
        AggregateId = aggregateId;
        Version = version;
    }

    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

    public Guid AggregateId { get; init; }
    public int Version { get; init; }
};

// TODO look into using record types
public record CustomerCreatedEvent(Guid AggregateId, string Name, string Email)
    : DomainEvent(AggregateId, 1);

public record KnowledgeTestUpdatedEvent(Guid AggregateId, bool Passed)
    : DomainEvent(AggregateId, 1);

public record PremiumCalculatedEvent(Guid CustomerId, decimal Amount) : DomainEvent(CustomerId, 1);

public interface IEventStore
{
    Task SaveEvents(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken);
    Task<IReadOnlyList<IDomainEvent>> GetEvents(
        Guid aggregateId,
        CancellationToken cancellationToken
    );
}
