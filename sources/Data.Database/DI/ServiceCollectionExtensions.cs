using Data.Database.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Data.Database.DI;

public static class ServiceCollectionExtensions
{
    public const string ConnectionStringName = "Default";

    /// <summary>
    /// Registers <see cref="MerkwerkDbContext"/> as a scoped DbContext factory (one context per unit of work, ADR 012).
    /// Needs an <c>ICurrentUser</c> registration (scoped) and the connection string <c>ConnectionStrings:Default</c>.
    /// </summary>
    public static IServiceCollection AddMerkwerkDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:Default is missing. Run deploy/setup-local.ps1 to create it as a user secret.");
        }

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<AuditSaveChangesInterceptor>();

        // Scoped (not the default singleton): the context and the interceptor depend on the scoped ICurrentUser.
        services.AddDbContextFactory<MerkwerkDbContext>(
            (serviceProvider, options) => options
                .UseMySQL(connectionString)
                .AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>()),
            ServiceLifetime.Scoped);

        // Identity's UserManager needs the context itself as a scoped service (one per request, from the same factory).
        services.TryAddScoped(serviceProvider =>
            serviceProvider.GetRequiredService<IDbContextFactory<MerkwerkDbContext>>().CreateDbContext());

        return services;
    }
}
