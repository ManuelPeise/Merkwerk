using Logic.Organizations.Groups;
using Logic.Organizations.Tests.Infrastructure;
using Logic.Shared.Interfaces;
using Shared.Enums;
using Shared.Models.Organizations;

namespace Logic.Organizations.Tests;

/// <summary>LP-108: groups of children – only admins change them, max. 20, never with children of another family.</summary>
[Collection(SharedDatabaseCollection.Name)]
public sealed class GroupServiceTests(SharedDatabaseFixture database)
{
    private readonly OrganizationsTestContext _context = OrganizationsTestContext.Create(database.ConnectionString);

    [Fact]
    public async Task CreateUpdateDelete_Admin_ChangesTheList()
    {
        // Arrange
        var family = await UseFamilyAsync();
        var mia = await CreateLearnerAsync(family, "Mia");
        var ben = await CreateLearnerAsync(family, "Ben");

        // Act
        var created = await CreateAsync(family, family.OwnerUserId, new GroupInput(" Beide ", [mia, ben]));
        var updated = await UpdateAsync(family, family.OwnerUserId, created.Group!.Id, new GroupInput("Nur Mia", [mia]));
        var afterUpdate = await ListAsync(family, family.OwnerUserId);
        var deleted = await DeleteAsync(family, family.OwnerUserId, created.Group.Id);

        // Assert
        Assert.Equal(GroupChangeStatus.Success, created.Status);
        Assert.Equal("Beide", created.Group.Name);
        Assert.Equal(new[] { Math.Min(mia, ben), Math.Max(mia, ben) }, created.Group.LearnerIds);
        Assert.Equal(GroupChangeStatus.Success, updated.Status);
        var group = Assert.Single(afterUpdate!);
        Assert.Equal("Nur Mia", group.Name);
        Assert.Equal(new[] { mia }, group.LearnerIds);
        Assert.Equal(GroupChangeStatus.Success, deleted);
        Assert.Empty((await ListAsync(family, family.OwnerUserId))!);
        Assert.Equal(2, (await ListLearnersAsync(family)).Count);
    }

    [Fact]
    public async Task CreateAsync_EmptyGroup_IsAllowed()
    {
        var family = await UseFamilyAsync();

        var created = await CreateAsync(family, family.OwnerUserId, new GroupInput("Später", []));

        Assert.Equal(GroupChangeStatus.Success, created.Status);
        Assert.Empty(created.Group!.LearnerIds);
    }

    [Fact]
    public async Task CreateAsync_NameTakenInOtherCase_ReportsName()
    {
        var family = await UseFamilyAsync();
        await CreateAsync(family, family.OwnerUserId, new GroupInput("Grundschule", []));

        var result = await CreateAsync(family, family.OwnerUserId, new GroupInput("grundschule", []));

        Assert.Equal(GroupChangeStatus.Invalid, result.Status);
        Assert.Contains("name", result.Errors!.Keys);
    }

    [Fact]
    public async Task CreateAsync_SameNameInOtherFamily_IsAllowed()
    {
        var familyA = await UseFamilyAsync();
        await CreateAsync(familyA, familyA.OwnerUserId, new GroupInput("Beide", []));
        var familyB = await UseFamilyAsync();

        var result = await CreateAsync(familyB, familyB.OwnerUserId, new GroupInput("Beide", []));

        Assert.Equal(GroupChangeStatus.Success, result.Status);
    }

    [Fact]
    public async Task CreateAsync_ChildOfOtherFamily_ReportsLearnerIds()
    {
        // Arrange: a child of family A; the admin of family B guesses its id.
        var familyA = await UseFamilyAsync();
        var childOfA = await CreateLearnerAsync(familyA, "Mia");
        var familyB = await UseFamilyAsync();
        var childOfB = await CreateLearnerAsync(familyB, "Ben");

        // Act
        var result = await CreateAsync(familyB, familyB.OwnerUserId, new GroupInput("Gemischt", [childOfB, childOfA]));

        // Assert
        Assert.Equal(GroupChangeStatus.Invalid, result.Status);
        Assert.Contains("learnerIds", result.Errors!.Keys);
        Assert.Empty((await ListAsync(familyB, familyB.OwnerUserId))!);
    }

    [Fact]
    public async Task CreateAsync_TwentyFirstGroup_LimitReached()
    {
        var family = await UseFamilyAsync();
        for (var i = 1; i <= GroupRules.MaxPerOrganization; i++)
        {
            Assert.Equal(GroupChangeStatus.Success, (await CreateAsync(family, family.OwnerUserId, new GroupInput($"Gruppe {i}", []))).Status);
        }

        var result = await CreateAsync(family, family.OwnerUserId, new GroupInput("Gruppe 21", []));

        Assert.Equal(GroupChangeStatus.LimitReached, result.Status);
    }

    [Fact]
    public async Task ChangesByMember_AreRejected_ButMemberSeesGroups()
    {
        // Arrange
        var family = await UseFamilyAsync();
        var mia = await CreateLearnerAsync(family, "Mia");
        var group = (await CreateAsync(family, family.OwnerUserId, new GroupInput("Beide", [mia]))).Group!;
        var (memberId, _) = await _context.CreateAccountAsync();
        await _context.AddMemberAsync(family.OrganizationId, memberId, OrganizationRole.Member);

        // Act
        var created = await CreateAsync(family, memberId, new GroupInput("Neu", []));
        var updated = await UpdateAsync(family, memberId, group.Id, new GroupInput("Umbenannt", []));
        var deleted = await DeleteAsync(family, memberId, group.Id);
        var seen = await ListAsync(family, memberId);

        // Assert
        Assert.Equal(GroupChangeStatus.NotFound, created.Status);
        Assert.Equal(GroupChangeStatus.NotFound, updated.Status);
        Assert.Equal(GroupChangeStatus.NotFound, deleted);
        var seenGroup = Assert.Single(seen!);
        Assert.Equal("Beide", seenGroup.Name);
        Assert.Equal(new[] { mia }, seenGroup.LearnerIds);
    }

    [Fact]
    public async Task UpdateAndDelete_GroupOfOtherFamily_NotFound()
    {
        var familyA = await UseFamilyAsync();
        var group = (await CreateAsync(familyA, familyA.OwnerUserId, new GroupInput("Beide", []))).Group!;
        var familyB = await UseFamilyAsync();

        var updated = await UpdateAsync(familyB, familyB.OwnerUserId, group.Id, new GroupInput("Gekapert", []));
        var deleted = await DeleteAsync(familyB, familyB.OwnerUserId, group.Id);

        Assert.Equal(GroupChangeStatus.NotFound, updated.Status);
        Assert.Equal(GroupChangeStatus.NotFound, deleted);
        _context.CurrentUser.OrganizationId = familyA.OrganizationId;
        Assert.Equal("Beide", Assert.Single((await ListAsync(familyA, familyA.OwnerUserId))!).Name);
    }

    [Fact]
    public async Task DeletedChild_DisappearsFromItsGroups()
    {
        // Arrange
        var family = await UseFamilyAsync();
        var mia = await CreateLearnerAsync(family, "Mia");
        var ben = await CreateLearnerAsync(family, "Ben");
        await CreateAsync(family, family.OwnerUserId, new GroupInput("Beide", [mia, ben]));

        // Act
        await _context.RunAsync<ILearnerService, LearnerChangeStatus>(s =>
            s.DeleteAsync(family.OrganizationId, family.OwnerUserId, mia, default));

        // Assert
        Assert.Equal(new[] { ben }, Assert.Single((await ListAsync(family, family.OwnerUserId))!).LearnerIds);
    }

    [Fact]
    public async Task ListAsync_AdultOfOtherFamily_ReturnsNull()
    {
        var familyA = await UseFamilyAsync();
        var familyB = await UseFamilyAsync();

        Assert.Null(await ListAsync(familyA, familyB.OwnerUserId));
    }

    /// <summary>New family; the "request" works in it from now on.</summary>
    private async Task<Family> UseFamilyAsync()
    {
        var family = await _context.CreateFamilyAsync();
        _context.CurrentUser.OrganizationId = family.OrganizationId;
        return family;
    }

    private async Task<long> CreateLearnerAsync(Family family, string name)
    {
        var result = await _context.RunAsync<ILearnerService, LearnerChangeResult>(s =>
            s.CreateAsync(family.OrganizationId, family.OwnerUserId, new LearnerInput(name, 2, "fox"), default));
        return result.Learner!.Id;
    }

    private async Task<IReadOnlyList<LearnerInfo>> ListLearnersAsync(Family family) =>
        (await _context.RunAsync<ILearnerService, IReadOnlyList<LearnerInfo>?>(s =>
            s.ListAsync(family.OrganizationId, family.OwnerUserId, default)))!;

    private Task<IReadOnlyList<GroupInfo>?> ListAsync(Family family, long actingUserId) =>
        _context.RunAsync<IGroupService, IReadOnlyList<GroupInfo>?>(s => s.ListAsync(family.OrganizationId, actingUserId, default));

    private Task<GroupChangeResult> CreateAsync(Family family, long actingUserId, GroupInput input) =>
        _context.RunAsync<IGroupService, GroupChangeResult>(s => s.CreateAsync(family.OrganizationId, actingUserId, input, default));

    private Task<GroupChangeResult> UpdateAsync(Family family, long actingUserId, long groupId, GroupInput input) =>
        _context.RunAsync<IGroupService, GroupChangeResult>(s => s.UpdateAsync(family.OrganizationId, actingUserId, groupId, input, default));

    private Task<GroupChangeStatus> DeleteAsync(Family family, long actingUserId, long groupId) =>
        _context.RunAsync<IGroupService, GroupChangeStatus>(s => s.DeleteAsync(family.OrganizationId, actingUserId, groupId, default));
}
