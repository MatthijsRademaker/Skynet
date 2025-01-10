using Arnold.CustomerContract;
using Microsoft.EntityFrameworkCore;

namespace Arnold.SkyNet;

public static class DependencyInjection
{
    public static void AddSkyNet(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<SkyNetDbContext>(
            "skyNetDb",
            configureDbContextOptions: dbContextOptionsBuilder =>
            {
                dbContextOptionsBuilder.UseNpgsql(builder =>
                {
                    builder.EnableRetryOnFailure();
                    builder.MigrationsAssembly("Arnold.SkyNet");
                });
            }
        );
    }
}
