using Arnold.CustomerContractDbContext;
using Arnold.SkyNet.Domain;
using Microsoft.EntityFrameworkCore;

namespace Arnold.CustomerContract
{
    /// <remarks>
    /// Add migrations using the following command inside the 'Skynet' project directory:
    ///
    /// dotnet ef migrations add --context SkyNetContext [migration-name]
    /// </remarks>
    public class SkyNetContext : DbContext
    {
        public SkyNetContext(DbContextOptions<SkyNetContext> options)
            : base(options) { }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<StoredEvent> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("core");
            builder.ApplyConfiguration(new CustomerTypeConfiguration());
            builder.ApplyConfiguration(new StoredEventTypeConfiguration());
            // Add the outbox table to this context
            // builder.UseIntegrationEventLogs();
        }

        public async Task MigrateAsync()
        {
            try
            {
                if (Database.IsNpgsql())
                {
                    await Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"An error occurred while migrating the database: {ex.Message}",
                    ex
                );
            }
        }
    }
}
