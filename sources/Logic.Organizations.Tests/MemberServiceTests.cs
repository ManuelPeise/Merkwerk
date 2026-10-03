using Logic.Authentication;
using Logic.Organizations.Tests.Infrastructure;
using Logic.Shared.Interfaces;
using Shared.Enums;
using Shared.Models.Organizations;

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

    [Fact]
    public async Task ListAsync_RemovedMember_ReturnsNull()
    {
        var family = await UseFamilyAsync();
        var (memberId, _) = await _context.CreateAccountAsync();
        var membershipId = await _context.AddMemberAsync(family.OrganizationId, memberId, OrganizationRole.Member);
        await RemoveAsync(family, family.OwnerUserId, membershipId);

        var members = await _context.RunAsync<IMemberService, IReadOnlyList<MemberInfo>?>(s =>
            s.ListAsync(family.OrganizationId, memberId, default));

        Assert.Null(members);
    }

    [Fact]
    public async Task IssueStartPasswordAsync_ByAdminForMember_MailsStartPassword()
    {
        var family = await UseFamilyAsync();
        var (memberId, _) = await _context.CreateAccountAsync();
        await _context.AddMemberAsync(family.OrganizationId, memberId, OrganizationRole.Member);

        var issued = await IssueStartPasswordAsync(family, family.OwnerUserId, memberId);

        Assert.True(issued);
        Assert.Single(_context.Mail.Sent, m => m.Template == MailTemplate.OneTimeCode);
    }

    [Fact]
    public async Task IssueStartPasswordAsync_ByMember_IsRejected()
    {
        var family = await UseFamilyAsync();
        var (memberId, _) = await _context.CreateAccountAsync();
        await _context.AddMemberAsync(family.OrganizationId, memberId, OrganizationRole.Member);

        var issued = await IssueStartPasswordAsync(family, memberId, family.OwnerUserId);

        Assert.False(issued);
        Assert.Empty(_context.Mail.Sent);
    }

    [Fact]
    public async Task IssueStartPasswordAsync_ByRemovedAdmin_IsRejected()
    {
        // The removed admin still holds a valid access token with the OrgAdmin claim for up to 15 minutes.
        var family = await UseFamilyAsync();
        var (adminId, _) = await _context.CreateAccountAsync();
        var adminMembership = await _context.AddMemberAsync(family.OrganizationId, adminId, OrganizationRole.OrgAdmin);
        var (memberId, _) = await _context.CreateAccountAsync();
        await _context.AddMemberAsync(family.OrganizationId, memberId, OrganizationRole.Member);
        await RemoveAsync(family, family.OwnerUserId, adminMembership);

        var issued = await IssueStartPasswordAsync(family, adminId, memberId);

        Assert.False(issued);
        Assert.Empty(_context.Mail.Sent);
    }

    [Fact]
    public async Task IssueStartPasswordAsync_TargetInOtherFamily_IsRejected()
    {
        var family = await UseFamilyAsync();
        var otherFamily = await _context.CreateFamilyAsync();

        var issued = await IssueStartPasswordAsync(family, family.OwnerUserId, otherFamily.OwnerUserId);

        Assert.False(issued);
        Assert.Empty(_context.Mail.Sent);
    }

    private async Task<Family> UseFamilyAsync()
    {
        var family = await _context.CreateFamilyAsync();
        _context.CurrentUser.OrganizationId = family.OrganizationId;
        return family;
    }

    private async Task<IReadOnlyList<MemberInfo>> ListAsync(Family family) =>
        (await _context.RunAsync<IMemberService, IReadOnlyList<MemberInfo>?>(s =>
            s.ListAsync(family.OrganizationId, family.OwnerUserId, default)))!;

    private Task<bool> IssueStartPasswordAsync(Family family, long actingUserId, long targetUserId) =>
        _context.RunAsync<IMemberService, bool>(s =>
            s.IssueStartPasswordAsync(family.OrganizationId, actingUserId, targetUserId, "de", default));

    private Task<RemoveMemberStatus> RemoveAsync(Family family, long actingUserId, long membershipId) =>
        _context.RunAsync<IMemberService, RemoveMemberStatus>(s =>
            s.RemoveAsync(family.OrganizationId, actingUserId, membershipId, default));
}
