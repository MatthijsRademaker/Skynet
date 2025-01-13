using System.Text.Json;
using Arnold.CustomerContract;
using Arnold.CustomerContract.Events;
using Arnold.SkyNet.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arnold.SkyNet.DomainEvents;

public class EventStore(ILogger<EventStore> logger, SkyNetContext context) : IEventStore
{
    private T DeserializeEvent<T>(string data, JsonSerializerOptions options)
        where T : IDomainEvent
    {
        return JsonSerializer.Deserialize<T>(data, options)!;
    }

    public async Task SaveEvents(
        IEnumerable<IDomainEvent> events,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation("Saving {EventCount} events", events.Count());
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            IncludeFields =
                true // This is important for fields like PremiumCalculatedEvent.Amount
            ,
        };
        var storedEvents = events.Select(e => new StoredEvent
        {
            Id = e.Id,
            AggregateId = e.AggregateId,
            Type = e.GetType().Name,
            Data = JsonSerializer.Serialize(e, e.GetType(), options),
            Timestamp = e.OccurredOn,
            Version = e.Version,
        });

        await context.Events.AddRangeAsync(storedEvents, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<IDomainEvent>> GetEvents(
        Guid aggregateId,
        CancellationToken cancellationToken
    )
    {
        var events = await context
            .Events.Where(e => e.AggregateId == aggregateId)
            .OrderBy(e => e.Version)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved {EventCount} events", events.Count);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            IncludeFields =
                true // Add this line
            ,
        };
        var domainEvents = events
            .Select<StoredEvent, IDomainEvent>(e =>
                e.Type switch
                {
                    nameof(CustomerCreatedEvent) => DeserializeEvent<CustomerCreatedEvent>(
                        e.Data,
                        options
                    ),
                    nameof(PremiumCalculatedEvent) => DeserializeEvent<PremiumCalculatedEvent>(
                        e.Data,
                        options
                    ),
                    _ => throw new InvalidOperationException($"Unknown event type: {e.Type}"),
                }
            )
            .ToList();

        return domainEvents;
    }
}
