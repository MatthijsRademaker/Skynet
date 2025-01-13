using Arnold.CustomerContract;
using Arnold.CustomerContract.Events;
using Arnold.SkyNet.Domain;
using MediatR;

namespace Arnold.SkyNet.CommandHandlers
{
    public abstract class DomainEventCommand : INotification
    {
        public abstract IDomainEvent @event { get; }
    }

    public class DomainEventHandler(IEventStore eventStore)
        : INotificationHandler<DomainEventCommand>
    {
        public async Task Handle(
            DomainEventCommand notification,
            CancellationToken cancellationToken
        )
        {
            await eventStore.SaveEvents(new[] { notification.@event }, cancellationToken);
        }
    }
}
