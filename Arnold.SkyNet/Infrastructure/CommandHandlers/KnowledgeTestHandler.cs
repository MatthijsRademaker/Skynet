using Arnold.CustomerContract;
using Arnold.CustomerContract.Events;
using Arnold.SkyNet.Domain;
using MediatR;

namespace Arnold.SkyNet.CommandHandlers
{
    public class KnowledgeTestCommandRequest(KnowledgeTestCommand knowledgeTestCommand)
        : DomainEventCommand
    // TODO maybe implement an interface such that i can create a generic event handler
    {
        public bool Passed => knowledgeTestCommand.Passed;

        public Guid AggregateId => knowledgeTestCommand.CustomerId;

        public override IDomainEvent @event =>
            new KnowledgeTestUpdatedEvent(AggregateId, knowledgeTestCommand.Passed);
    }

    public class KnowledgeTestCommandHandler(
        ILogger<KnowledgeTestCommandHandler> logger,
        ICustomerRepository customerRepository
    ) : INotificationHandler<KnowledgeTestCommandRequest>
    {
        public async Task Handle(
            KnowledgeTestCommandRequest command,
            CancellationToken cancellationToken
        )
        {
            logger.LogInformation("Handling {CommandName} command", nameof(KnowledgeTestCommand));

            logger.LogInformation(
                "Updating knowledge test for customer {Id} with {Passed}",
                command.AggregateId,
                command.Passed
            );
            var customer = await customerRepository.GetAsync(
                command.AggregateId,
                cancellationToken
            );
            customer.UpdateKnowledgeTest(command.Passed);
            await customerRepository.SaveAsync(customer, cancellationToken);
        }
    }
}
