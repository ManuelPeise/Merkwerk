using Data.Database.Entities.Organizations;

namespace Logic.Organizations.Invitations;

/// <summary>Adults join a family only by invitation (LP-105): 7 days valid, single use, revocable, token stored hashed.</summary>
public interface IInvitationService
{
    /// <summary>Creates the invitation and mails the link. A newer invitation replaces an open one for the same address.</summary>
    Task<CreateInvitationResult> CreateAsync(
        long organizationId,
        long actingUserId,
        string email,
        OrganizationRole role,
        string language,
        CancellationToken cancellationToken);

    /// <summary>Open (pending or expired, not used, not withdrawn) invitations of the organization; null if the acting user is no admin.</summary>
    Task<IReadOnlyList<InvitationInfo>?> ListAsync(long organizationId, long actingUserId, CancellationToken cancellationToken);

    /// <summary>False if the invitation does not exist in this organization or the acting user is no admin.</summary>
    Task<bool> RevokeAsync(long organizationId, long actingUserId, long invitationId, CancellationToken cancellationToken);

    Task<InvitationDetailsResult> GetDetailsAsync(string token, CancellationToken cancellationToken);

    /// <summary><paramref name="currentUserId"/> is the signed-in adult, or null for a new account.</summary>
    Task<AcceptInvitationResult> AcceptAsync(
        AcceptInvitationRequest request,
        long? currentUserId,
        CancellationToken cancellationToken);
}
