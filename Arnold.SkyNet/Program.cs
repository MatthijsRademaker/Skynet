using Arnold.CustomerContract;
using Arnold.SkyNet;

var builder = Host.CreateApplicationBuilder(args);
builder.AddSkyNet();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SkyNetContext>();
        await context.MigrateAsync();
    }
    catch (Exception ex)
    {
        // Log the error or handle it as needed
        Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
    }
}
host.Run();
