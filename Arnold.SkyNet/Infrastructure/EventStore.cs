using System.Text.Json;
using Arnold.CustomerContract;
using Arnold.SkyNet.Domain;
using Microsoft.EntityFrameworkCore;

namespace Arnold.SkyNet.DomainEvents;

public class EventStore : IEventStore
{
    private readonly SkyNetContext _context;

    public EventStore(SkyNetContext context)
    {
        _context = context;
    }

    public async Task SaveEvents(
        IEnumerable<IDomainEvent> events,
        CancellationToken cancellationToken
    )
    {
        var storedEvents = events.Select(e => new StoredEvent
        {
            Id = e.Id,
            AggregateId = e.AggregateId,
            Type = e.GetType().Name,
            Data = JsonSerializer.Serialize(e),
            Timestamp = e.OccurredOn,
            Version = e.Version,
        });

        await _context.Events.AddRangeAsync(storedEvents, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<IDomainEvent>> GetEvents(
        Guid aggregateId,
        CancellationToken cancellationToken
    )
    {
        var events = await _context
            .Events.Where(e => e.AggregateId == aggregateId)
            .OrderBy(e => e.Version)
            .ToListAsync(cancellationToken);

        return events
            .Select(e =>
            {
                var eventType = Type.GetType($"Arnold.CustomerContract.Events.{e.Type}");
                var domainEvent = (IDomainEvent)JsonSerializer.Deserialize(e.Data, eventType)!;
                domainEvent.Version = e.Version;
                return domainEvent;
            })
            .ToList();
    }
}
