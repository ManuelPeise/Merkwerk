using Data.Accessor.Abstractions;
using Data.Database.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Data.Accessor.DI;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the database (<see cref="Data.Database.DI.ServiceCollectionExtensions.AddMerkwerkDatabase"/>) and
    /// <see cref="IUnitOfWorkFactory"/>. Needs a scoped <c>ICurrentUser</c> registration.
    /// </summary>
    public static IServiceCollection AddMerkwerkDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMerkwerkDatabase(configuration);

        // Scoped like the DbContext factory it wraps (that one depends on the scoped ICurrentUser).
        services.TryAddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();

        return services;
    }
}
