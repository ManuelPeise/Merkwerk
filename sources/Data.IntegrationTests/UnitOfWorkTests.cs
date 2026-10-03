using Data.Accessor;
using Data.Accessor.Abstractions;
using Data.Database.Entities.Organizations;
using Data.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Data.IntegrationTests;

/// <summary>LP-102: repositories and unit of work against a real MySQL 8.4.</summary>
public sealed class UnitOfWorkTests(MySqlFixture database) : IClassFixture<MySqlFixture>
{
    [Fact]
    public async Task SaveChangesAsync_AddedEntity_IsStoredAndLoadableById()
    {
        // Arrange
        long id;
        await using (var uow = CreateUnitOfWork(new TestCurrentUser("user:1")))
        {
            var organization = new Organization { Name = "Familie Repo" };
            uow.Organizations.Add(organization);

            // Act
            var written = await uow.SaveChangesAsync();

            Assert.Equal(1, written);
            id = organization.Id;
        }

        // Assert
        await using var reader = CreateUnitOfWork(new TestCurrentUser());
        var saved = await reader.Organizations.GetByIdAsync(id);
        Assert.NotNull(saved);
        Assert.Equal("Familie Repo", saved.Name);
        Assert.Equal("user:1", saved.CreatedBy);
    }

    [Fact]
    public async Task GetByIdAsync_EntityOfOtherOrganization_ReturnsNull()
    {
        // Arrange
        var (familyA, familyB) = await CreateTwoOrganizationsAsync();
        var noteId = await CreateNoteAsync(familyB, "B only");

        // Act
        await using var uowA = CreateUnitOfWork(new TestCurrentUser("user:1", familyA));
        await using var uowB = CreateUnitOfWork(new TestCurrentUser("user:2", familyB));
        var seenByA = await uowA.Repository<TestNote>().GetByIdAsync(noteId);
        var seenByB = await uowB.Repository<TestNote>().GetByIdAsync(noteId);

        // Assert
        Assert.Null(seenByA);
        Assert.NotNull(seenByB);
    }

    [Fact]
    public async Task Query_ChangedEntity_IsNotSaved()
    {
        // Arrange
        var id = await CreateOrganizationAsync("Familie Lesen");

        // Act
        await using (var uow = CreateUnitOfWork(new TestCurrentUser()))
        {
            var organization = await uow.Organizations.Query().SingleAsync(o => o.Id == id);
            organization.Name = "Familie Geaendert";
            var written = await uow.SaveChangesAsync();

            Assert.Equal(0, written);
        }

        // Assert
        Assert.Equal("Familie Lesen", await ReadNameAsync(id));
    }

    [Fact]
    public async Task QueryTracked_ChangedEntity_IsSaved()
    {
        // Arrange
        var id = await CreateOrganizationAsync("Familie Vorher");

        // Act
        await using (var uow = CreateUnitOfWork(new TestCurrentUser("user:3")))
        {
            var organization = await uow.Organizations.QueryTracked().SingleAsync(o => o.Id == id);
            organization.Name = "Familie Nachher";
            await uow.SaveChangesAsync();
        }

        // Assert
        Assert.Equal("Familie Nachher", await ReadNameAsync(id));
    }

    [Fact]
    public async Task Remove_SaveChangesAsync_DeletesEntity()
    {
        // Arrange
        var id = await CreateOrganizationAsync("Familie Weg");

        // Act
        await using (var uow = CreateUnitOfWork(new TestCurrentUser()))
        {
            var organization = await uow.Organizations.GetByIdAsync(id);
            uow.Organizations.Remove(organization!);
            await uow.SaveChangesAsync();
        }

        // Assert
        await using var reader = CreateUnitOfWork(new TestCurrentUser());
        Assert.Null(await reader.Organizations.GetByIdAsync(id));
    }

    [Fact]
    public async Task SaveChangesAsync_UnsavedChangesInOtherUnitOfWork_WritesNothing()
    {
        // Arrange: the first unit of work adds but never saves.
        await using var first = CreateUnitOfWork(new TestCurrentUser());
        first.Organizations.Add(new Organization { Name = "Familie Ungespeichert" });

        // Act
        await using var second = CreateUnitOfWork(new TestCurrentUser());
        var written = await second.SaveChangesAsync();

        // Assert
        Assert.Equal(0, written);
        Assert.False(await second.Organizations.Query().AnyAsync(o => o.Name == "Familie Ungespeichert"));
    }

    [Fact]
    public async Task Repository_SameType_ReturnsSameInstanceAsSpecializedRepository()
    {
        await using var uow = CreateUnitOfWork(new TestCurrentUser());

        Assert.Same(uow.Organizations, uow.Repository<Organization>());
        Assert.Same(uow.Repository<TestNote>(), uow.Repository<TestNote>());
    }

    [Fact]
    public async Task AnyAsync_OrganizationExists_ReturnsTrue()
    {
        await CreateOrganizationAsync("Familie Setup");

        await using var uow = CreateUnitOfWork(new TestCurrentUser());

        Assert.True(await uow.Organizations.AnyAsync());
    }

    private IUnitOfWork CreateUnitOfWork(TestCurrentUser user) =>
        new UnitOfWorkFactory(new TestDbContextFactory(database.ConnectionString, user)).Create();

    private async Task<long> CreateOrganizationAsync(string name)
    {
        await using var uow = CreateUnitOfWork(new TestCurrentUser());
        var organization = new Organization { Name = name };
        uow.Organizations.Add(organization);
        await uow.SaveChangesAsync();
        return organization.Id;
    }

    private async Task<(long A, long B)> CreateTwoOrganizationsAsync() =>
        (await CreateOrganizationAsync("Familie A"), await CreateOrganizationAsync("Familie B"));

    private async Task<long> CreateNoteAsync(long organizationId, string text)
    {
        await using var uow = CreateUnitOfWork(new TestCurrentUser());
        var note = new TestNote { OrganizationId = organizationId, Text = text };
        uow.Repository<TestNote>().Add(note);
        await uow.SaveChangesAsync();
        return note.Id;
    }

    private async Task<string> ReadNameAsync(long id)
    {
        await using var uow = CreateUnitOfWork(new TestCurrentUser());
        return await uow.Organizations.Query().Where(o => o.Id == id).Select(o => o.Name).SingleAsync();
    }
}
