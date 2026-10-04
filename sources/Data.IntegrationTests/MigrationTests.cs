using Data.Database.Entities.Subjects;
using Data.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Data.IntegrationTests;

/// <summary>LP-103: the migrations build the real schema on an empty MySQL 8.4 and seed the standard subjects.</summary>
public sealed class MigrationTests(MigratedMySqlFixture database) : IClassFixture<MigratedMySqlFixture>
{
    [Fact]
    public async Task Migrate_EmptyDatabase_AppliesAllMigrations()
    {
        await using var context = database.CreateContext();

        Assert.Empty(await context.Database.GetPendingMigrationsAsync());
        Assert.NotEmpty(await context.Database.GetAppliedMigrationsAsync());
    }

    [Fact]
    public async Task Migrate_EmptyDatabase_SeedsStandardSubjects()
    {
        await using var context = database.CreateContext();

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
        using var context = database.CreateContext();

        Assert.False(context.Database.HasPendingModelChanges());
    }

    private static void AssertSubject(SubjectEntity subject, string name, string languageCode, string color)
    {
        Assert.Equal(name, subject.Name);
        Assert.Equal(languageCode, subject.LanguageCode);
        Assert.Equal(color, subject.Color);
        Assert.Equal("system", subject.CreatedBy);
    }
}
