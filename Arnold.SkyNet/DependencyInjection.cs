using Arnold.CustomerContract;
using Arnold.CustomerContract.Events;
using Arnold.SkyNet.Domain;
using Arnold.SkyNet.DomainEvents;
using Arnold.SkyNet.Infrastructure;
using Arnold.SkyNet.Migrations;
using Microsoft.EntityFrameworkCore;

namespace Arnold.SkyNet;

public static class DependencyInjection
{
    public static void AddSkyNet(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<SkyNetContext>(
            "skynetdb",
            configureDbContextOptions: dbContextOptionsBuilder =>
            {
                dbContextOptionsBuilder.UseNpgsql(builder =>
                {
                    builder.EnableRetryOnFailure();
                    builder.MigrationsAssembly("Arnold.SkyNet");
                });
            }
        );

        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly)
        );

        builder.AddAzureServiceBusClient("messaging");
        builder.Services.AddHostedService<Worker>();
        builder.Services.AddHostedService<DatabaseMigrationService>();
        builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
        builder.Services.AddScoped<IEventStore, EventStore>();
    }
}
