using Arnold.CustomerContract;
using Arnold.CustomerContract.Events;
using Arnold.SkyNet.Domain;
using Arnold.SkyNet.DomainEvents;
using Arnold.SkyNet.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Arnold.KnowledgeTest;

public static class DependencyInjection
{
    public static void AddKnowledgeTest(this IHostApplicationBuilder builder)
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
