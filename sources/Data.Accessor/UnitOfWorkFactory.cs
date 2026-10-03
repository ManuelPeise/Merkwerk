using Data.Accessor.Abstractions;
using Data.Database;
using Microsoft.EntityFrameworkCore;

namespace Data.Accessor;

/// <summary>Every unit of work gets a fresh DbContext from the (scoped) context factory – no long-lived contexts.</summary>
internal sealed class UnitOfWorkFactory(IDbContextFactory<MerkwerkDbContext> contextFactory) : IUnitOfWorkFactory
{
    public IUnitOfWork Create() => new UnitOfWork(contextFactory.CreateDbContext());
}
