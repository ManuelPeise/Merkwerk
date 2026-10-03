using Data.Database.Entities.Organizations;
using Data.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Data.IntegrationTests;

/// <summary>LP-101: tenant filter, audit fields, UTC handling and character set against a real MySQL 8.4.</summary>
public sealed class MerkwerkDbContextTests(MySqlFixture database) : IClassFixture<MySqlFixture>
{
    private static readonly DateTimeOffset Start = new(2026, 10, 3, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Query_TwoOrganizations_ReturnsOnlyRowsOfCurrentOrganization()
    {
        // Arrange: two families with one note each (the system actor writes; inserts are not filtered).
        var (familyA, familyB) = await CreateTwoOrganizationsAsync();
        await using (var writer = CreateContext(new TestCurrentUser()))
        {
            writer.Notes.AddRange(
                new TestNote { OrganizationId = familyA, Text = "A" },
                new TestNote { OrganizationId = familyB, Text = "B" });
            await writer.SaveChangesAsync();
        }

        // Act
        await using var readerA = CreateContext(new TestCurrentUser("user:1", familyA));
        await using var readerB = CreateContext(new TestCurrentUser("user:2", familyB));
        var notesA = await readerA.Notes.Where(n => n.Text == "A" || n.Text == "B").ToListAsync();
        var notesB = await readerB.Notes.Where(n => n.Text == "A" || n.Text == "B").ToListAsync();

        // Assert
        Assert.All(notesA, n => Assert.Equal(familyA, n.OrganizationId));
        Assert.All(notesB, n => Assert.Equal(familyB, n.OrganizationId));
        Assert.Contains(notesA, n => n.Text == "A");
        Assert.DoesNotContain(notesA, n => n.Text == "B");
    }

    [Fact]
    public async Task Query_NoOrganization_ReturnsNoRows()
    {
        var (familyA, _) = await CreateTwoOrganizationsAsync();
        await using (var writer = CreateContext(new TestCurrentUser()))
        {
            writer.Notes.Add(new TestNote { OrganizationId = familyA, Text = "hidden" });
            await writer.SaveChangesAsync();
        }

        await using var reader = CreateContext(new TestCurrentUser("system", organizationId: null));

        Assert.Empty(await reader.Notes.ToListAsync());
    }

    [Fact]
    public async Task SaveChanges_Added_SetsAllAuditFields()
    {
        var time = new ManualTimeProvider(Start);
        await using var context = CreateContext(new TestCurrentUser("user:7"), time);
        var organization = new Organization { Name = "Familie Audit" };

        context.Organizations.Add(organization);
        await context.SaveChangesAsync();

        Assert.Equal(Start.UtcDateTime, organization.CreatedAt);
        Assert.Equal("user:7", organization.CreatedBy);
        Assert.Equal(Start.UtcDateTime, organization.UpdatedAt);
        Assert.Equal("user:7", organization.UpdatedBy);
    }

    [Fact]
    public async Task SaveChanges_Modified_KeepsCreatedAndUpdatesUpdated()
    {
        // Arrange
        var time = new ManualTimeProvider(Start);
        long id;
        await using (var creator = CreateContext(new TestCurrentUser("user:1"), time))
        {
            var organization = new Organization { Name = "Familie Alt" };
            creator.Organizations.Add(organization);
            await creator.SaveChangesAsync();
            id = organization.Id;
        }

        time.Advance(TimeSpan.FromHours(2));

        // Act: someone else renames it and even tries to overwrite the creation data.
        await using (var editor = CreateContext(new TestCurrentUser("user:2"), time))
        {
            var organization = await editor.Organizations.SingleAsync(o => o.Id == id);
            organization.Name = "Familie Neu";
            organization.CreatedBy = "user:999";
            await editor.SaveChangesAsync();
        }

        // Assert
        await using var reader = CreateContext(new TestCurrentUser());
        var saved = await reader.Organizations.SingleAsync(o => o.Id == id);
        Assert.Equal("Familie Neu", saved.Name);
        Assert.Equal("user:1", saved.CreatedBy);
        Assert.Equal(Start.UtcDateTime, saved.CreatedAt);
        Assert.Equal("user:2", saved.UpdatedBy);
        Assert.Equal(Start.AddHours(2).UtcDateTime, saved.UpdatedAt);
    }

    [Fact]
    public async Task DateTime_RoundTrip_IsReadAsUtc()
    {
        var (familyA, _) = await CreateTwoOrganizationsAsync();
        var dueAt = new DateTime(2026, 12, 24, 18, 30, 0, DateTimeKind.Utc);
        long id;
        await using (var writer = CreateContext(new TestCurrentUser()))
        {
            var note = new TestNote { OrganizationId = familyA, Text = "utc", DueAt = dueAt };
            writer.Notes.Add(note);
            await writer.SaveChangesAsync();
            id = note.Id;
        }

        await using var reader = CreateContext(new TestCurrentUser("user:1", familyA));
        var saved = await reader.Notes.SingleAsync(n => n.Id == id);

        Assert.Equal(DateTimeKind.Utc, saved.CreatedAt.Kind);
        Assert.Equal(DateTimeKind.Utc, saved.DueAt!.Value.Kind);
        Assert.Equal(dueAt, saved.DueAt.Value);
    }

    [Fact]
    public async Task Database_CharacterSet_IsUtf8mb4()
    {
        await using var context = CreateContext(new TestCurrentUser());

        var characterSet = await context.Database
            .SqlQueryRaw<string>("SELECT @@character_set_database AS `Value`")
            .SingleAsync();

        Assert.Equal("utf8mb4", characterSet);
    }

    private TestDbContext CreateContext(TestCurrentUser user, TimeProvider? time = null) =>
        TestDbContext.Create(database.ConnectionString, user, time);

    private async Task<(long A, long B)> CreateTwoOrganizationsAsync()
    {
        await using var context = CreateContext(new TestCurrentUser());
        var familyA = new Organization { Name = "Familie A" };
        var familyB = new Organization { Name = "Familie B" };
        context.Organizations.AddRange(familyA, familyB);
        await context.SaveChangesAsync();
        return (familyA.Id, familyB.Id);
    }
}
