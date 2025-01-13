using Arnold.CustomerContract.Events;

namespace Arnold.SkyNet.Domain;

public class Customer
{
    public Guid? Id { get; }
    public string? Name { get; }
    public string? Email { get; }

    // TODO sorted set?
    public List<Premium> PremiumAmounts { get; private set; }

    public Customer(Guid id, string name, string email)
    {
        Id = id;
        Name = name;
        Email = email;
    }

    private Customer() { }

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
                    customer = new Customer(
                        customerCreatedEvent.AggregateId,
                        customerCreatedEvent.Name,
                        customerCreatedEvent.Email
                    );
                    break;
                case PremiumCalculatedEvent premiumCalculatedEvent:
                    customer.AddPremium(premiumCalculatedEvent.Amount);
                    break;
            }
        }

        return customer;
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
