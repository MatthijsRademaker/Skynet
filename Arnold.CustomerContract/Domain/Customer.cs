using Arnold.CustomerContract.Events;

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

    // TODO move to separate class
    public static Customer RecreateCustomer(IReadOnlyList<IDomainEvent> events)
    {
        var customer = new Customer();
        foreach (var @event in events)
        {
            switch (@event)
            {
                case CustomerCreatedEvent customerCreatedEvent:
                    customer = CreateCustomer(
                        customerCreatedEvent.AggregateId,
                        customerCreatedEvent.Name,
                        customerCreatedEvent.Email
                    );
                    break;
                case PremiumCalculatedEvent premiumCalculatedEvent:
                    customer.AddPremium(premiumCalculatedEvent.Amount);
                    break;
                case KnowledgeTestUpdatedEvent knowledgeTestUpdatedEvent:
                    customer.UpdateKnowledgeTest(knowledgeTestUpdatedEvent.Passed);
                    break;
            }
        }

        return customer;
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
