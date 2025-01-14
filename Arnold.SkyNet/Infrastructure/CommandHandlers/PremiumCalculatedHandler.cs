using Arnold.CustomerContract;
using Arnold.CustomerContract.Events;
using Arnold.SkyNet.Domain;
using MediatR;

namespace Arnold.SkyNet.CommandHandlers
{
    public class PremiumCalculatedCommandNotification(PremiumCalculatedCommand command)
        : DomainEventCommand
    // TODO maybe implement an interface such that i can create a generic event handler
    {
        public Premium Premium => command.Premium;

        public Guid AggregateId => command.Id;

        public override IDomainEvent @event =>
            new PremiumCalculatedEvent(AggregateId, command.Premium.Amount);
    }

    public class PremiumCalculatedCommandHandler(
        ILogger<PremiumCalculatedCommandHandler> logger,
        ICustomerRepository customerRepository
    ) : INotificationHandler<PremiumCalculatedCommandNotification>
    {
        public async Task Handle(
            PremiumCalculatedCommandNotification command,
            CancellationToken cancellationToken
        )
        {
            logger.LogInformation(
                "Handling {CommandName} command",
                nameof(PremiumCalculatedCommand)
            );

            logger.LogInformation(
                "Updating premium amount for customer {Id} with {Passed}",
                command.AggregateId,
                command.Premium.Amount
            );
            var customer = await customerRepository.GetAsync(
                command.AggregateId,
                cancellationToken
            );
            customer.AddPremium(command.Premium.Amount);
            await customerRepository.SaveAsync(customer, cancellationToken);
        }
    }
}
