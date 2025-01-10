using Arnold.CustomerContractDbContext;
using Microsoft.EntityFrameworkCore;

namespace Arnold.CustomerContract
{
    public class SkyNetDbContext : DbContext
    {
        public SkyNetDbContext(DbContextOptions<SkyNetDbContext> options)
            : base(options) { }

        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("core");
            builder.ApplyConfiguration(new CustomerTypeConfiguration());
            // Add the outbox table to this context
            // builder.UseIntegrationEventLogs();
        }
    }
}
