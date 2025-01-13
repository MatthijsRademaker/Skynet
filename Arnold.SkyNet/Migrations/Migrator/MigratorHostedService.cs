using Arnold.CustomerContract;

namespace Arnold.SkyNet.Migrations;

public class DatabaseMigrationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseMigrationService> _logger;

    public DatabaseMigrationService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseMigrationService> logger
    )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SkyNetContext>();

        try
        {
            await context.MigrateAsync();
            _logger.LogInformation("Database migration completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while migrating the database");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
