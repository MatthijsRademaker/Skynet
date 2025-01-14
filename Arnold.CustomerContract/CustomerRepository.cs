using Arnold.CustomerContract;
using Arnold.SkyNet.Domain;

namespace Arnold.SkyNet.Infrastructure
{
    public class CustomerRepository(SkyNetContext skyNetDbContext) : ICustomerRepository
    {
        public Task<Customer> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            // TODO: Implement cancellation token usage
            // TODO: Implement error handling
            return Task.FromResult(skyNetDbContext.Customers.Find(id));
        }

        public Task<Customer?> GetAsync(string email, CancellationToken cancellationToken)
        {
            return Task.FromResult(skyNetDbContext.Customers.FirstOrDefault(c => c.Email == email));
        }

        public async Task SaveAsync(Customer customer, CancellationToken cancellationToken)
        {
            var existingCustomer = skyNetDbContext.Customers.FirstOrDefault(c =>
                c.Id == customer.Id
            );

            if (existingCustomer is not null)
            {
                skyNetDbContext.Update(customer);
            }
            else
            {
                skyNetDbContext.Customers.Add(customer);
            }

            await skyNetDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
