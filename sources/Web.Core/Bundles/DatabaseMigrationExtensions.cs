using Data.Database;
using Microsoft.EntityFrameworkCore;

namespace Web.Core.Bundles;

/// <summary>Applies pending EF Core migrations when the app starts (ADR 016), called once from Program.cs.</summary>
public static partial class DatabaseMigrationExtensions
{
    /// <summary>
    /// Brings the database to the latest migration before the app accepts requests. Existing data stays – migrations
    /// only change the schema step by step. If a migration fails the exception ends the start, so the app never runs
    /// against a half-migrated database (MySQL does not run DDL transactionally – restore the backup in that case).
    /// </summary>
    public static async Task MigrateDatabaseAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<MerkwerkDbContext>().Database;

        var pendingMigrations = (await database.GetPendingMigrationsAsync(cancellationToken)).ToList();

        if (pendingMigrations.Count == 0)
        {
            LogDatabaseUpToDate(app.Logger);
            return;
        }

        LogApplyingMigrations(app.Logger, pendingMigrations.Count, string.Join(", ", pendingMigrations));
        await database.MigrateAsync(cancellationToken);
        LogMigrationsApplied(app.Logger);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Database schema is up to date")]
    private static partial void LogDatabaseUpToDate(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Applying {Count} pending migration(s): {Migrations}")]
    private static partial void LogApplyingMigrations(ILogger logger, int count, string migrations);

    [LoggerMessage(Level = LogLevel.Information, Message = "Migrations applied")]
    private static partial void LogMigrationsApplied(ILogger logger);
}
