namespace Data.Accessor.Abstractions;

/// <summary>Creates a new <see cref="IUnitOfWork"/> per business operation. Inject this, never the unit of work itself.</summary>
public interface IUnitOfWorkFactory
{
    IUnitOfWork Create();
}
