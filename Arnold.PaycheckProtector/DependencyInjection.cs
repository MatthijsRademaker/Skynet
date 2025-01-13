using Arnold.CustomerContract;
using Arnold.CustomerContract.Events;
using Arnold.SkyNet.Domain;
using Arnold.SkyNet.DomainEvents;
using Arnold.SkyNet.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Arnold.PaycheckProtector;

public static class DependencyInjection
{
    public static void AddPaycheckProtector(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<SkyNetContext>(
            "skynetdb",
            configureDbContextOptions: dbContextOptionsBuilder =>
            {
                dbContextOptionsBuilder.UseNpgsql(builder =>
                {
                    builder.EnableRetryOnFailure();
                });
            }
        );

        builder.AddAzureServiceBusClient("messaging");
        builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
        builder.Services.AddScoped<IEventStore, EventStore>();
    }
}
