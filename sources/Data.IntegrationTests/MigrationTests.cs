using Data.Database;
using Data.Database.Entities.Subjects;
using Data.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MySql;

namespace Data.IntegrationTests;

/// <summary>LP-103: the migrations build the real schema on an empty MySQL 8.4 and seed the standard subjects.</summary>
public sealed class MigrationTests : IAsyncLifetime
{
    // Own container: the shared MySqlFixture creates its schema with EnsureCreated, which cannot be migrated.
    private readonly MySqlContainer _container = new MySqlBuilder()
        .WithImage("mysql:8.4")
        .Build();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    [Fact]
    public async Task MigrateAsync_EmptyDatabase_AppliesAllMigrations()
    {
        await using var context = CreateContext();

        await context.Database.MigrateAsync();

        Assert.Empty(await context.Database.GetPendingMigrationsAsync());
        Assert.NotEmpty(await context.Database.GetAppliedMigrationsAsync());
    }

    [Fact]
    public async Task MigrateAsync_EmptyDatabase_SeedsStandardSubjects()
    {
        await using var context = CreateContext();

        await context.Database.MigrateAsync();
        var subjects = await context.Subjects.OrderBy(s => s.Id).ToListAsync();

        Assert.Collection(
            subjects,
            s => AssertSubject(s, "Deutsch", "de", "subject.german"),
            s => AssertSubject(s, "Englisch", "en", "subject.english"),
            s => AssertSubject(s, "Mathe", "de", "subject.math"));
    }

    [Fact]
    public void Model_AfterLastMigration_HasNoPendingChanges()
    {
        // Fails when an entity or configuration changed without `dotnet ef migrations add`.
        using var context = CreateContext();

        Assert.False(context.Database.HasPendingModelChanges());
    }

    private MerkwerkDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MerkwerkDbContext>()
            .UseMySQL(_container.GetConnectionString())
            .Options;

        return new MerkwerkDbContext(options, new TestCurrentUser());
    }

    private static void AssertSubject(Subject subject, string name, string languageCode, string color)
    {
        Assert.Equal(name, subject.Name);
        Assert.Equal(languageCode, subject.LanguageCode);
        Assert.Equal(color, subject.Color);
        Assert.Equal("system", subject.CreatedBy);
    }
}
