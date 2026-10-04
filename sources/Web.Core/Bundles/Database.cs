using Data.Database;
using Microsoft.EntityFrameworkCore;

namespace Web.Core.Bundles;

public static class Database
{
    /// <summary>
    /// Ensures the database is migrated to the latest EF Core migration.
    /// This method builds a short-lived service provider and runs migrations inside a scope.
    /// Note: calling EnsureCreated together with migrations is not supported and therefore
    /// this method only applies pending migrations.
    /// </summary>
    public static async Task EnsureDatabaseCreated(this IServiceCollection services)
    {
        // Build a short-lived provider to run startup migrations. ValidateScopes helps catch
        // scope-related errors early when called from the composition root.
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var scope = provider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<MerkwerkDbContext>();

        var database = dbContext.Database;
        var pendingMigrations = await database.GetPendingMigrationsAsync();

        if (pendingMigrations.Any())
        {
            await database.MigrateAsync();
        }
    }
}
