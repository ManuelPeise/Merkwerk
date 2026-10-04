using Data.Accessor.Abstractions;
using Data.Database;
using Microsoft.EntityFrameworkCore;

namespace Data.Accessor;

/// <summary>Every unit of work gets a fresh DbContext from the (scoped) context factory – no long-lived contexts.</summary>
internal sealed class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IDbContextFactory<MerkwerkDbContext> _contextFactory;

    public UnitOfWorkFactory(IDbContextFactory<MerkwerkDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public IUnitOfWork Create() => new UnitOfWork(_contextFactory.CreateDbContext());
}
