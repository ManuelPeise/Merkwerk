using Logic.Organizations.Learners;
using Logic.Organizations.Tests.Infrastructure;
using Logic.Shared.Interfaces;
using Shared.Enums;
using Shared.Models.Organizations;

namespace Logic.Organizations.Tests;

/// <summary>LP-105: child profiles – only admins change them, max. 10, never across families.</summary>
[Collection(SharedDatabaseCollection.Name)]
public sealed class LearnerServiceTests(SharedDatabaseFixture database)
{
    private readonly OrganizationsTestContext _context = OrganizationsTestContext.Create(database.ConnectionString);

    [Fact]
    public async Task CreateUpdateDelete_Admin_ChangesTheList()
    {
        // Arrange
        var family = await UseFamilyAsync();

        // Act
        var created = await CreateAsync(family, new LearnerInput(" Mia ", 2, "fox"));
        var updated = await _context.RunAsync<ILearnerService, LearnerChangeResult>(s => s.UpdateAsync(
            family.OrganizationId, family.OwnerUserId, created.Learner!.Id, new LearnerInput("Mia", 3, "owl"), default));
        var afterUpdate = await ListAsync(family);
        var deleted = await _context.RunAsync<ILearnerService, LearnerChangeStatus>(s => s.DeleteAsync(
            family.OrganizationId, family.OwnerUserId, created.Learner!.Id, default));

        // Assert
        Assert.Equal(LearnerChangeStatus.Success, created.Status);
        Assert.Equal("Mia", created.Learner!.DisplayName);
        Assert.Equal(LearnerChangeStatus.Success, updated.Status);
        Assert.Equal(new LearnerInfo(created.Learner.Id, "Mia", 3, "owl"), Assert.Single(afterUpdate));
        Assert.Equal(LearnerChangeStatus.Success, deleted);
        Assert.Empty(await ListAsync(family));
    }

    [Fact]
    public async Task CreateAsync_EleventhChild_LimitReached()
    {
        var family = await UseFamilyAsync();
        for (var i = 1; i <= LearnerRules.MaxPerOrganization; i++)
        {
            Assert.Equal(LearnerChangeStatus.Success, (await CreateAsync(family, new LearnerInput($"Kind {i}", 1, "cat"))).Status);
        }

        var eleventh = await CreateAsync(family, new LearnerInput("Kind 11", 1, "cat"));

        Assert.Equal(LearnerChangeStatus.LimitReached, eleventh.Status);
    }

    [Theory]
    [InlineData("", 2, "fox", "displayName")]
    [InlineData("Mia", 5, "fox", "grade")]
    [InlineData("Mia", 2, "dragon", "avatarId")]
    public async Task CreateAsync_InvalidInput_ReportsField(string name, int grade, string avatarId, string field)
    {
        var family = await UseFamilyAsync();

        var result = await CreateAsync(family, new LearnerInput(name, grade, avatarId));

        Assert.Equal(LearnerChangeStatus.Invalid, result.Status);
        Assert.Contains(field, result.Errors!.Keys);
    }

    [Fact]
    public async Task CreateAsync_ByMember_IsRejected()
    {
        var family = await UseFamilyAsync();
        var (memberId, _) = await _context.CreateAccountAsync();
        await _context.AddMemberAsync(family.OrganizationId, memberId, OrganizationRole.Member);

        var result = await _context.RunAsync<ILearnerService, LearnerChangeResult>(s => s.CreateAsync(
            family.OrganizationId, memberId, new LearnerInput("Mia", 2, "fox"), default));

        Assert.Equal(LearnerChangeStatus.NotFound, result.Status);
        Assert.Empty(await ListAsync(family));
    }

    [Fact]
    public async Task UpdateAsync_LearnerOfOtherFamily_NotFound()
    {
        // Arrange: a child of family A; the admin of family B guesses its id.
        var familyA = await UseFamilyAsync();
        var learner = (await CreateAsync(familyA, new LearnerInput("Mia", 2, "fox"))).Learner!;
        var familyB = await UseFamilyAsync();

        // Act
        var result = await _context.RunAsync<ILearnerService, LearnerChangeResult>(s => s.UpdateAsync(
            familyB.OrganizationId, familyB.OwnerUserId, learner.Id, new LearnerInput("Hacked", 1, "pig"), default));

        // Assert
        Assert.Equal(LearnerChangeStatus.NotFound, result.Status);
        _context.CurrentUser.OrganizationId = familyA.OrganizationId;
        Assert.Equal("Mia", Assert.Single(await ListAsync(familyA)).DisplayName);
    }

    [Fact]
    public async Task UpdateAndDeleteAsync_ByMember_AreRejected()
    {
        // Arrange: the admin created a child; a plain member tries to change it (LP-107).
        var family = await UseFamilyAsync();
        var learner = (await CreateAsync(family, new LearnerInput("Mia", 2, "fox"))).Learner!;
        var (memberId, _) = await _context.CreateAccountAsync();
        await _context.AddMemberAsync(family.OrganizationId, memberId, OrganizationRole.Member);

        // Act
        var updated = await _context.RunAsync<ILearnerService, LearnerChangeResult>(s => s.UpdateAsync(
            family.OrganizationId, memberId, learner.Id, new LearnerInput("Lea", 1, "owl"), default));
        var deleted = await _context.RunAsync<ILearnerService, LearnerChangeStatus>(s => s.DeleteAsync(
            family.OrganizationId, memberId, learner.Id, default));

        // Assert
        Assert.Equal(LearnerChangeStatus.NotFound, updated.Status);
        Assert.Equal(LearnerChangeStatus.NotFound, deleted);
        Assert.Equal("Mia", Assert.Single(await ListAsync(family)).DisplayName);
    }

    [Fact]
    public async Task ListAsync_ByMember_ReturnsChildren()
    {
        var family = await UseFamilyAsync();
        await CreateAsync(family, new LearnerInput("Mia", 2, "fox"));
        var (memberId, _) = await _context.CreateAccountAsync();
        await _context.AddMemberAsync(family.OrganizationId, memberId, OrganizationRole.Member);

        var learners = await _context.RunAsync<ILearnerService, IReadOnlyList<LearnerInfo>?>(s =>
            s.ListAsync(family.OrganizationId, memberId, default));

        Assert.Single(learners!);
    }

    [Fact]
    public async Task DeleteAsync_LearnerOfOtherFamily_NotFound()
    {
        // Arrange: a child of family A; the admin of family B guesses its id.
        var familyA = await UseFamilyAsync();
        var learner = (await CreateAsync(familyA, new LearnerInput("Mia", 2, "fox"))).Learner!;
        var familyB = await UseFamilyAsync();

        // Act
        var deleted = await _context.RunAsync<ILearnerService, LearnerChangeStatus>(s => s.DeleteAsync(
            familyB.OrganizationId, familyB.OwnerUserId, learner.Id, default));

        // Assert
        Assert.Equal(LearnerChangeStatus.NotFound, deleted);
        _context.CurrentUser.OrganizationId = familyA.OrganizationId;
        Assert.Single(await ListAsync(familyA));
    }

    [Fact]
    public async Task ListAsync_AdultOfOtherFamily_ReturnsNull()
    {
        var familyA = await UseFamilyAsync();
        var familyB = await UseFamilyAsync();

        var learners = await _context.RunAsync<ILearnerService, IReadOnlyList<LearnerInfo>?>(s =>
            s.ListAsync(familyA.OrganizationId, familyB.OwnerUserId, default));

        Assert.Null(learners);
    }

    /// <summary>New family; the "request" works in it from now on.</summary>
    private async Task<Family> UseFamilyAsync()
    {
        var family = await _context.CreateFamilyAsync();
        _context.CurrentUser.OrganizationId = family.OrganizationId;
        return family;
    }

    private Task<LearnerChangeResult> CreateAsync(Family family, LearnerInput input) =>
        _context.RunAsync<ILearnerService, LearnerChangeResult>(s =>
            s.CreateAsync(family.OrganizationId, family.OwnerUserId, input, default));

    private async Task<IReadOnlyList<LearnerInfo>> ListAsync(Family family) =>
        (await _context.RunAsync<ILearnerService, IReadOnlyList<LearnerInfo>?>(s =>
            s.ListAsync(family.OrganizationId, family.OwnerUserId, default)))!;
}
