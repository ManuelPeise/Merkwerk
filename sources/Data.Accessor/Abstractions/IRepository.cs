using Data.Database.Entities.Base;

namespace Data.Accessor.Abstractions;

/// <summary>
/// Generic access to one entity type inside a unit of work (ADR 012). Organization-scoped entities are filtered
/// to the current organization automatically; services still check ownership (AGENTS.md §5).
/// </summary>
public interface IRepository<T>
    where T : AEntityBase
{
    /// <summary>Tracked entity (changes are saved), or <c>null</c> if it does not exist or belongs to another organization.</summary>
    Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Read-only query (no tracking) – project into DTOs with <c>Select</c>.</summary>
    IQueryable<T> Query();

    /// <summary>Tracked query for loading entities that will be changed and saved.</summary>
    IQueryable<T> QueryTracked();

    /// <summary>Marks the entity for insertion on <see cref="IUnitOfWork.SaveChangesAsync"/>. Audit fields are set there.</summary>
    void Add(T entity);

    /// <summary>Marks the entity for deletion on <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Remove(T entity);
}
