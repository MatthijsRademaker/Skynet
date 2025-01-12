using Arnold.CustomerContract;
using Arnold.SkyNet.Domain;
using MediatR;

namespace Arnold.SkyNet.CommandHandlers
{
    public class CreateCustomerCommandRequest(CreateCustomerCommand createCustomerCommand)
        : DomainEventCommand
    // TODO maybe implement an interface such that i can create a generic event handler
    {
        public CreateCustomerCommand CreateCustomerCommand { get; } = createCustomerCommand;

        public override IDomainEvent @event =>
            new CustomerCreatedEvent(
                Guid.NewGuid(),
                CreateCustomerCommand.Name,
                CreateCustomerCommand.Email
            );
    }

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

    public class CustomerCreatedHandler(
        ILogger<CustomerCreatedHandler> logger,
        ICustomerRepository customerRepository
    ) : INotificationHandler<CreateCustomerCommandRequest>
    {
        public async Task Handle(
            CreateCustomerCommandRequest request,
            CancellationToken cancellationToken
        )
        {
            logger.LogInformation("Handling {CommandName} command", nameof(CreateCustomerCommand));
            var customer = new Customer(
                request.@event.AggregateId,
                request.CreateCustomerCommand.Name,
                request.CreateCustomerCommand.Email
            );
            await customerRepository.SaveAsync(customer, cancellationToken);
        }
    }
}
