using System.Diagnostics;

using CleanPatternWithCloudNative.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CleanPatternWithCloudNative.MigrationService
{
    public class Worker(
        IServiceProvider serviceProvider,
        IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
    {
        public const string ActivitySourceName = "Migrations";
        public static readonly ActivitySource ActivitySource = new(ActivitySourceName);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using Activity? activity = ActivitySource.StartActivity("Migrating Database", ActivityKind.Client);

            try
            {
                using IServiceScope scope = serviceProvider.CreateScope();
                ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(stoppingToken);
                if (pendingMigrations.Any())
                {
                    await dbContext.Database.MigrateAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                activity?.AddException(ex);
                throw;
            }

            hostApplicationLifetime.StopApplication();
        }
    }
}
