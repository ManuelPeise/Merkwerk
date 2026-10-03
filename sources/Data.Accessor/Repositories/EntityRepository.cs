using Data.Accessor.Abstractions;
using Data.Database;
using Data.Database.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace Data.Accessor.Repositories;

/// <summary>Default repository; specialized repositories derive from it.</summary>
internal class EntityRepository<T>(MerkwerkDbContext context) : IRepository<T>
    where T : AEntityBase
{
    protected DbSet<T> Set { get; } = context.Set<T>();

    public Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        Set.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    public IQueryable<T> Query() => Set.AsNoTracking();

    public IQueryable<T> QueryTracked() => Set;

    public void Add(T entity) => Set.Add(entity);

    public void Remove(T entity) => Set.Remove(entity);
}
