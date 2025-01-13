using Arnold.CustomerContract;
using Arnold.CustomerContract.Events;
using Arnold.SkyNet.Domain;
using MediatR;

namespace Arnold.SkyNet.CommandHandlers
{
    public class CreateCustomerCommandRequest(CreateCustomerCommand createCustomerCommand)
        : DomainEventCommand
    // TODO maybe implement an interface such that i can create a generic event handler
    {
        public string Name => createCustomerCommand.Name;

        public string Email => createCustomerCommand.Email;

        public Guid AggregateId => createCustomerCommand.CustomerId;

        public override IDomainEvent @event =>
            new CustomerCreatedEvent(
                AggregateId,
                createCustomerCommand.Name,
                createCustomerCommand.Email
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
            CreateCustomerCommandRequest command,
            CancellationToken cancellationToken
        )
        {
            logger.LogInformation("Handling {CommandName} command", nameof(CreateCustomerCommand));
            logger.LogInformation(
                "Creating customer {Name} with email {Email} and {Id}",
                command.Name,
                command.Email,
                command.AggregateId
            );
            var customer = new Customer(command.AggregateId, command.Name, command.Email);
            await customerRepository.SaveAsync(customer, cancellationToken);
        }
    }
}
