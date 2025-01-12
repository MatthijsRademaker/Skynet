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

        public async Task SaveAsync(Customer customer, CancellationToken cancellationToken)
        {
            skyNetDbContext.Customers.Add(customer);
            await skyNetDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
