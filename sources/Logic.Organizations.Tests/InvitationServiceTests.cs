using Data.Database.Entities.Organizations;
using Logic.Authentication;
using Logic.Notifications;
using Logic.Organizations.Invitations;
using Logic.Organizations.Members;
using Logic.Organizations.Tests.Infrastructure;

namespace Logic.Organizations.Tests;

/// <summary>LP-105: invitations – 7 days, single use, revocable, only by admins, only into the own family.</summary>
[Collection(SharedDatabaseCollection.Name)]
public sealed class InvitationServiceTests(SharedDatabaseFixture database)
{
    private readonly OrganizationsTestContext _context = OrganizationsTestContext.Create(database.ConnectionString);

    [Fact]
    public async Task AcceptAsync_NewAccountFromMailLink_JoinsFamilyAsMember()
    {
        // Arrange
        var family = await _context.CreateFamilyAsync("Familie Sonnenschein");
        var email = $"{Guid.NewGuid():N}@example.org";
        await InviteAsync(family, email);
        var token = _context.Mail.LinkValue(MailTemplate.Invitation, "token");

        // Act
        var details = await _context.RunAsync<IInvitationService, InvitationDetailsResult>(s => s.GetDetailsAsync(token, default));
        var accepted = await AcceptAsync(token, currentUserId: null, displayName: "Ben");

        // Assert
        Assert.Equal(InvitationLookupStatus.Found, details.Status);
        Assert.Equal("Familie Sonnenschein", details.Details!.FamilyName);
        Assert.Equal("Owner", details.Details.InvitedBy);
        Assert.Equal(AcceptInvitationStatus.Success, accepted.Status);
        Assert.Equal(AuthRoles.Member, accepted.Session!.Role);

        _context.CurrentUser.OrganizationId = family.OrganizationId;
        var members = await _context.RunAsync<IMemberService, IReadOnlyList<MemberInfo>>(s => s.ListAsync(family.OrganizationId, default));
        Assert.Contains(members, m => m.DisplayName == "Ben" && m.Role == OrganizationRole.Member);
    }

    [Fact]
    public async Task AcceptAsync_UsedTwice_SecondIsGone()
    {
        var family = await _context.CreateFamilyAsync();
        await InviteAsync(family, $"{Guid.NewGuid():N}@example.org");
        var token = _context.Mail.LinkValue(MailTemplate.Invitation, "token");
        await AcceptAsync(token, null, "Ben");

        var second = await AcceptAsync(token, null, "Ben");

        Assert.Equal(AcceptInvitationStatus.Gone, second.Status);
    }

    [Fact]
    public async Task AcceptAsync_AfterSevenDays_IsGone()
    {
        var family = await _context.CreateFamilyAsync();
        await InviteAsync(family, $"{Guid.NewGuid():N}@example.org");
        var token = _context.Mail.LinkValue(MailTemplate.Invitation, "token");

        _context.Time.Advance(TimeSpan.FromDays(7).Add(TimeSpan.FromMinutes(1)));

        Assert.Equal(AcceptInvitationStatus.Gone, (await AcceptAsync(token, null, "Ben")).Status);
    }

    [Fact]
    public async Task AcceptAsync_Revoked_IsGone()
    {
        var family = await _context.CreateFamilyAsync();
        var invitation = await InviteAsync(family, $"{Guid.NewGuid():N}@example.org");
        var token = _context.Mail.LinkValue(MailTemplate.Invitation, "token");

        var revoked = await _context.RunAsync<IInvitationService, bool>(s =>
            s.RevokeAsync(family.OrganizationId, family.OwnerUserId, invitation.Id, default));

        Assert.True(revoked);
        Assert.Equal(AcceptInvitationStatus.Gone, (await AcceptAsync(token, null, "Ben")).Status);
    }

    [Fact]
    public async Task AcceptAsync_AnonymousButAccountExists_AsksToSignIn()
    {
        var family = await _context.CreateFamilyAsync();
        var (_, email) = await _context.CreateAccountAsync();
        await InviteAsync(family, email);

        var result = await AcceptAsync(_context.Mail.LinkValue(MailTemplate.Invitation, "token"), null, "Ben");

        Assert.Equal(AcceptInvitationStatus.AccountExists, result.Status);
    }

    [Fact]
    public async Task AcceptAsync_SignedInWithInvitedAddress_AddsMembershipWithInvitedRole()
    {
        var family = await _context.CreateFamilyAsync();
        var (userId, email) = await _context.CreateAccountAsync();
        await InviteAsync(family, email, OrganizationRole.OrgAdmin);

        var result = await AcceptAsync(_context.Mail.LinkValue(MailTemplate.Invitation, "token"), userId, displayName: null);

        Assert.Equal(AcceptInvitationStatus.Success, result.Status);
        Assert.Equal(AuthRoles.OrgAdmin, result.Session!.Role);
    }

    [Fact]
    public async Task AcceptAsync_SignedInWithOtherAddress_IsRejected()
    {
        var family = await _context.CreateFamilyAsync();
        var (otherUserId, _) = await _context.CreateAccountAsync();
        await InviteAsync(family, $"{Guid.NewGuid():N}@example.org");

        var result = await AcceptAsync(_context.Mail.LinkValue(MailTemplate.Invitation, "token"), otherUserId, null);

        Assert.Equal(AcceptInvitationStatus.EmailMismatch, result.Status);
    }

    [Fact]
    public async Task GetDetailsAsync_UnknownToken_NotFound()
    {
        var result = await _context.RunAsync<IInvitationService, InvitationDetailsResult>(s => s.GetDetailsAsync("unknown", default));

        Assert.Equal(InvitationLookupStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task CreateAsync_ByMember_IsForbidden()
    {
        var family = await _context.CreateFamilyAsync();
        var (memberId, _) = await _context.CreateAccountAsync();
        await _context.AddMemberAsync(family.OrganizationId, memberId, OrganizationRole.Member);

        var result = await _context.RunAsync<IInvitationService, CreateInvitationResult>(s => s.CreateAsync(
            family.OrganizationId, memberId, $"{Guid.NewGuid():N}@example.org", OrganizationRole.Member, "de", default));

        Assert.Equal(CreateInvitationStatus.Forbidden, result.Status);
        Assert.Empty(_context.Mail.Sent);
    }

    [Fact]
    public async Task CreateAsync_AddressAlreadyInFamily_IsRejected()
    {
        var family = await _context.CreateFamilyAsync();

        var result = await _context.RunAsync<IInvitationService, CreateInvitationResult>(s => s.CreateAsync(
            family.OrganizationId, family.OwnerUserId, family.OwnerEmail, OrganizationRole.Member, "de", default));

        Assert.Equal(CreateInvitationStatus.AlreadyMember, result.Status);
    }

    [Fact]
    public async Task RevokeAsync_InvitationOfOtherFamily_IsRejected()
    {
        // Arrange: family A invites, the admin of family B tries to withdraw it by id.
        var familyA = await _context.CreateFamilyAsync();
        var familyB = await _context.CreateFamilyAsync();
        var invitation = await InviteAsync(familyA, $"{Guid.NewGuid():N}@example.org");

        // Act
        _context.CurrentUser.OrganizationId = familyB.OrganizationId;
        var revoked = await _context.RunAsync<IInvitationService, bool>(s =>
            s.RevokeAsync(familyB.OrganizationId, familyB.OwnerUserId, invitation.Id, default));

        // Assert
        Assert.False(revoked);
        var token = _context.Mail.LinkValue(MailTemplate.Invitation, "token");
        Assert.Equal(InvitationLookupStatus.Found,
            (await _context.RunAsync<IInvitationService, InvitationDetailsResult>(s => s.GetDetailsAsync(token, default))).Status);
    }

    private async Task<InvitationInfo> InviteAsync(Family family, string email, OrganizationRole role = OrganizationRole.Member)
    {
        _context.CurrentUser.OrganizationId = family.OrganizationId;
        var result = await _context.RunAsync<IInvitationService, CreateInvitationResult>(s =>
            s.CreateAsync(family.OrganizationId, family.OwnerUserId, email, role, "de", default));

        Assert.Equal(CreateInvitationStatus.Created, result.Status);
        return result.Invitation!;
    }

    private Task<AcceptInvitationResult> AcceptAsync(string token, long? currentUserId, string? displayName) =>
        _context.RunAsync<IInvitationService, AcceptInvitationResult>(s => s.AcceptAsync(
            new AcceptInvitationRequest(token, displayName, OrganizationsTestContext.Password, PrivacyAccepted: true),
            currentUserId,
            default));
}
