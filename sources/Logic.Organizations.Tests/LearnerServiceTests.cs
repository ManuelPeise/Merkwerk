using Data.Database.Entities.Organizations;
using Logic.Organizations.Learners;
using Logic.Organizations.Tests.Infrastructure;

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

    private Task<IReadOnlyList<LearnerInfo>> ListAsync(Family family) =>
        _context.RunAsync<ILearnerService, IReadOnlyList<LearnerInfo>>(s => s.ListAsync(family.OrganizationId, default));
}
