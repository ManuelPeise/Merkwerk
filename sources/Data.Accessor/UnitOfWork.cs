using Data.Accessor.Abstractions;
using Data.Accessor.Repositories;
using Data.Database;
using Data.Database.Entities.Base;
using Data.Database.Entities.Organizations;

namespace Data.Accessor;

internal sealed class UnitOfWork(MerkwerkDbContext context) : IUnitOfWork
{
    // One repository instance per entity type and unit of work.
    private readonly Dictionary<Type, object> _repositories = [];

    public IOrganizationRepository Organizations => (IOrganizationRepository)Repository<Organization>();

    public IRepository<T> Repository<T>()
        where T : AEntityBase
    {
        if (!_repositories.TryGetValue(typeof(T), out var repository))
        {
            repository = CreateRepository<T>();
            _repositories.Add(typeof(T), repository);
        }

        return (IRepository<T>)repository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);

    public ValueTask DisposeAsync() => context.DisposeAsync();

    /// <summary>Register every specialized repository here, so both access paths return the same instance.</summary>
    private object CreateRepository<T>()
        where T : AEntityBase
    {
        if (typeof(T) == typeof(Organization))
        {
            return new OrganizationRepository(context);
        }

        return new EntityRepository<T>(context);
    }
}
