using Data.Database;
using Data.Database.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Data.IntegrationTests.Infrastructure;

/// <summary>Stands in for the registered context factory, so units of work run on <see cref="TestDbContext"/>.</summary>
public sealed class TestDbContextFactory(string connectionString, ICurrentUser currentUser) : IDbContextFactory<MerkwerkDbContext>
{
    public MerkwerkDbContext CreateDbContext() => TestDbContext.Create(connectionString, currentUser);
}
