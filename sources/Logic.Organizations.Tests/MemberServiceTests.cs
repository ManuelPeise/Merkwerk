using Data.Database.Entities.Organizations;
using Logic.Authentication;
using Logic.Organizations.Members;
using Logic.Organizations.Tests.Infrastructure;

namespace Logic.Organizations.Tests;

/// <summary>LP-105: adults of a family; the owner stays.</summary>
[Collection(SharedDatabaseCollection.Name)]
public sealed class MemberServiceTests(SharedDatabaseFixture database)
{
    private readonly OrganizationsTestContext _context = OrganizationsTestContext.Create(database.ConnectionString);

    [Fact]
    public async Task ListAsync_FamilyWithOwnerAndMember_ReturnsBothWithRoles()
    {
        var family = await UseFamilyAsync();
        var (memberId, _) = await _context.CreateAccountAsync("Ben");
        await _context.AddMemberAsync(family.OrganizationId, memberId, OrganizationRole.Member);

        var members = await ListAsync(family);

        Assert.Collection(
            members,
            owner => Assert.True(owner.IsOwner && owner.Role == OrganizationRole.OrgAdmin),
            member => Assert.True(member.DisplayName == "Ben" && member.Role == OrganizationRole.Member && !member.IsOwner));
    }

    [Fact]
    public async Task RemoveAsync_Owner_IsRejected()
    {
        var family = await UseFamilyAsync();
        var (adminId, _) = await _context.CreateAccountAsync();
        await _context.AddMemberAsync(family.OrganizationId, adminId, OrganizationRole.OrgAdmin);
        var ownerMembership = (await ListAsync(family)).Single(m => m.IsOwner).MembershipId;

        var status = await RemoveAsync(family, adminId, ownerMembership);

        Assert.Equal(RemoveMemberStatus.IsOwner, status);
    }

    [Fact]
    public async Task RemoveAsync_Member_RemovesMembershipAndRoleFromNextSession()
    {
        var family = await UseFamilyAsync();
        var (memberId, _) = await _context.CreateAccountAsync();
        var membershipId = await _context.AddMemberAsync(family.OrganizationId, memberId, OrganizationRole.Member);

        var status = await RemoveAsync(family, family.OwnerUserId, membershipId);

        Assert.Equal(RemoveMemberStatus.Success, status);
        Assert.DoesNotContain(await ListAsync(family), m => m.UserId == memberId);
        Assert.Equal(AuthRoles.Member, (await _context.SignInAsync(memberId))!.Role);
    }

    [Fact]
    public async Task RemoveAsync_ByMember_NotFound()
    {
        var family = await UseFamilyAsync();
        var (memberId, _) = await _context.CreateAccountAsync();
        var membershipId = await _context.AddMemberAsync(family.OrganizationId, memberId, OrganizationRole.Member);
        var (otherId, _) = await _context.CreateAccountAsync();
        var otherMembership = await _context.AddMemberAsync(family.OrganizationId, otherId, OrganizationRole.Member);

        var status = await RemoveAsync(family, memberId, otherMembership);

        Assert.Equal(RemoveMemberStatus.NotFound, status);
        Assert.Contains(await ListAsync(family), m => m.MembershipId == membershipId);
    }

    private async Task<Family> UseFamilyAsync()
    {
        var family = await _context.CreateFamilyAsync();
        _context.CurrentUser.OrganizationId = family.OrganizationId;
        return family;
    }

    private Task<IReadOnlyList<MemberInfo>> ListAsync(Family family) =>
        _context.RunAsync<IMemberService, IReadOnlyList<MemberInfo>>(s => s.ListAsync(family.OrganizationId, default));

    private Task<RemoveMemberStatus> RemoveAsync(Family family, long actingUserId, long membershipId) =>
        _context.RunAsync<IMemberService, RemoveMemberStatus>(s =>
            s.RemoveAsync(family.OrganizationId, actingUserId, membershipId, default));
}
