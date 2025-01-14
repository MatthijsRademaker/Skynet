using Arnold.CustomerContract.Events;
using Microsoft.Extensions.Logging;

namespace Arnold.SkyNet.Domain;

public class Customer
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }

    // TODO private set -> create DTO for dbcontext
    public bool KnowledgeTestPassed { get; set; }

    // TODO sorted set?
    public List<Premium> PremiumAmounts { get; set; }

    public static Customer CreateCustomer(Guid id, string name, string email)
    {
        return new Customer()
        {
            Id = id,
            Name = name,
            Email = email,
        };
    }

    public void AddPremium(decimal amount)
    {
        PremiumAmounts.Add(new Premium(amount));
    }

    public void UpdateKnowledgeTest(bool passed)
    {
        KnowledgeTestPassed = passed;
    }
}

public class Premium(decimal amount)
{
    public decimal Amount { get; } = amount;
}

public interface ICustomerRepository
{
    Task<Customer> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<Customer?> GetAsync(string email, CancellationToken cancellationToken);
    Task SaveAsync(Customer customer, CancellationToken cancellationToken);
}

public interface IPremiumCalculator
{
    // TODO should this be in the domain? Other services might not need to do this
    Task<Premium?> CalculatePremiumAsync(Guid customerId, CancellationToken cancellationToken);
}

public class CustomerFactory(
    ILogger<CustomerFactory> logger,
    IEventStore eventStore,
    IPremiumCalculator premiumCalculator
)
{
    public async Task<Customer> RecreateCustomer(
        Guid customerId,
        CancellationToken cancellationToken
    )
    {
        var events = await eventStore.GetEvents(customerId, cancellationToken);

        logger.LogInformation("Recreating customer {customerId} with {events}", customerId, events);
        // TODO create a find for customer created event and guerantee it exists
        var createdEvent = events.OfType<CustomerCreatedEvent>().First();
        var customer = Customer.CreateCustomer(
            createdEvent.AggregateId,
            createdEvent.Name,
            createdEvent.Email
        );

        logger.LogInformation("Recreating customer {customerId} with {events}", customerId, events);
        var remainingEvents = events.Where(e =>
            e.GetType() == typeof(CustomerCreatedEvent) && (CustomerCreatedEvent)e != createdEvent
        );
        logger.LogInformation("remainging events {events}", events);

        foreach (var @event in remainingEvents)
        {
            switch (@event)
            {
                case PremiumCalculatedEvent premiumCalculatedEvent:
                    logger.LogInformation(
                        "Adding premium {amount} to customer {customerId}",
                        premiumCalculatedEvent.Amount,
                        customerId
                    );
                    customer.AddPremium(premiumCalculatedEvent.Amount);
                    var premium = await premiumCalculator.CalculatePremiumAsync(
                        customer.Id.Value,
                        cancellationToken
                    );

                    customer.AddPremium(premium.Amount);
                    break;
                case KnowledgeTestUpdatedEvent knowledgeTestUpdatedEvent:
                    customer.UpdateKnowledgeTest(knowledgeTestUpdatedEvent.Passed);
                    break;
            }
        }

        return customer;
    }
}
