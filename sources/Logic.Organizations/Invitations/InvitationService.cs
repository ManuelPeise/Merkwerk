using System.Security.Cryptography;
using System.Text;
using Data.Accessor.Abstractions;
using Data.Database.Entities.Identity;
using Data.Database.Entities.Organizations;
using Logic.Notifications;
using Logic.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Enums;
using Shared.Models.Authentication;
using Shared.Models.Notifications;
using Shared.Models.Organizations;

namespace Logic.Organizations.Invitations;

internal sealed partial class InvitationService : IInvitationService
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(7);

    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IAccountService _accounts;
    private readonly IAuthSessionService _sessions;
    private readonly IMemberService _members;
    private readonly IMailService _mailService;
    private readonly IPublicLinkBuilder _links;
    private readonly IMailDateFormatter _dates;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<InvitationService> _logger;

    public InvitationService(
        IUnitOfWorkFactory unitOfWorkFactory,
        IAccountService accounts,
        IAuthSessionService sessions,
        IMemberService members,
        IMailService mailService,
        IPublicLinkBuilder links,
        IMailDateFormatter dates,
        TimeProvider timeProvider,
        ILogger<InvitationService> logger)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _accounts = accounts;
        _sessions = sessions;
        _members = members;
        _mailService = mailService;
        _links = links;
        _dates = dates;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<CreateInvitationResult> CreateAsync(
        long organizationId,
        long actingUserId,
        string email,
        OrganizationRole role,
        string language,
        CancellationToken cancellationToken)
    {
        if (!await _members.IsAdminAsync(organizationId, actingUserId, cancellationToken))
        {
            return new CreateInvitationResult(CreateInvitationStatus.Forbidden);
        }

        var normalizedEmail = email.Trim();
        if (await _accounts.FindUserIdByEmailAsync(normalizedEmail, cancellationToken) is { } existingUserId
            && await _members.IsMemberAsync(organizationId, existingUserId, cancellationToken))
        {
            return new CreateInvitationResult(CreateInvitationStatus.AlreadyMember);
        }

        var inviter = (await _accounts.GetAccountsAsync([actingUserId], cancellationToken)).Single();
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var now = _timeProvider.GetUtcNow();
        var expiresAt = now.Add(Lifetime);

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var organization = await unitOfWork.Organizations.GetByIdAsync(organizationId, cancellationToken)
            ?? throw new InvalidOperationException($"Organization {organizationId} does not exist.");

        // A new invitation for the same address replaces the open one.
        var open = await unitOfWork.Invitations.QueryTracked()
            .Where(i => i.OrganizationId == organizationId && i.Email == normalizedEmail
                && i.AcceptedAt == null && i.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var replaced in open)
        {
            replaced.RevokedAt = now.UtcDateTime;
        }

        var invitation = new InvitationEntity
        {
            OrganizationId = organizationId,
            Email = normalizedEmail,
            Role = role,
            TokenHash = Hash(token),
            ExpiresAt = expiresAt.UtcDateTime,
            InvitedByUserId = actingUserId,
            InvitedByName = inviter.DisplayName,
        };
        unitOfWork.Invitations.Add(invitation);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await _mailService.SendAsync(
                new MailMessageRequest(normalizedEmail, normalizedEmail, MailTemplate.Invitation, language,
                    new Dictionary<string, string>
                    {
                        ["Name"] = normalizedEmail,
                        ["InvitedBy"] = inviter.DisplayName,
                        ["OrganizationName"] = organization.Name,
                        ["Link"] = _links.Build("/invitation", ("token", token)),
                        ["ExpiresAt"] = _dates.Format(expiresAt, language),
                    }),
                cancellationToken);
        }
        catch (MailDeliveryException)
        {
            // The invitation stays; the admin sees it in the list and can invite again.
            LogMailFailed(invitation.Id);
        }

        return new CreateInvitationResult(CreateInvitationStatus.Created, ToInfo(invitation, now));
    }

    public async Task<IReadOnlyList<InvitationInfo>?> ListAsync(
        long organizationId,
        long actingUserId,
        CancellationToken cancellationToken)
    {
        // Checked against the database, not just the role claim: a removed admin keeps the claim for up to 15 minutes.
        if (!await _members.IsAdminAsync(organizationId, actingUserId, cancellationToken))
        {
            return null;
        }

        var now = _timeProvider.GetUtcNow();

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var open = await unitOfWork.Invitations.Query()
            .Where(i => i.OrganizationId == organizationId && i.AcceptedAt == null && i.RevokedAt == null)
            .OrderByDescending(i => i.Id)
            .ToListAsync(cancellationToken);

        return open.Select(i => ToInfo(i, now)).ToList();
    }

    public async Task<bool> RevokeAsync(
        long organizationId,
        long actingUserId,
        long invitationId,
        CancellationToken cancellationToken)
    {
        if (!await _members.IsAdminAsync(organizationId, actingUserId, cancellationToken))
        {
            return false;
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var invitation = await unitOfWork.Invitations.GetByIdAsync(invitationId, cancellationToken);

        // Explicit tenant check in addition to the query filter (ADR 007).
        if (invitation is null || invitation.OrganizationId != organizationId || invitation.AcceptedAt is not null)
        {
            return false;
        }

        invitation.RevokedAt ??= _timeProvider.GetUtcNow().UtcDateTime;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<InvitationDetailsResult> GetDetailsAsync(string token, CancellationToken cancellationToken)
    {
        await using var unitOfWork = _unitOfWorkFactory.Create();
        var invitation = await unitOfWork.Invitations.FindByTokenHashAsync(Hash(token), cancellationToken);

        if (invitation is null)
        {
            return new InvitationDetailsResult(InvitationLookupStatus.NotFound);
        }

        if (!IsUsable(invitation))
        {
            return new InvitationDetailsResult(InvitationLookupStatus.Gone);
        }

        var organization = await unitOfWork.Organizations.GetByIdAsync(invitation.OrganizationId, cancellationToken);

        return new InvitationDetailsResult(
            InvitationLookupStatus.Found,
            new InvitationDetails(
                organization?.Name ?? string.Empty,
                invitation.Email,
                invitation.InvitedByName,
                new DateTimeOffset(invitation.ExpiresAt, TimeSpan.Zero)));
    }

    public async Task<AcceptInvitationResult> AcceptAsync(
        AcceptInvitationRequest request,
        long? currentUserId,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = _unitOfWorkFactory.Create();
        var invitation = await unitOfWork.Invitations.FindByTokenHashAsync(Hash(request.Token), cancellationToken);

        if (invitation is null)
        {
            return new AcceptInvitationResult(AcceptInvitationStatus.NotFound);
        }

        if (!IsUsable(invitation))
        {
            return new AcceptInvitationResult(AcceptInvitationStatus.Gone);
        }

        long userId;
        var accountCreated = false;
        if (currentUserId is { } signedIn)
        {
            var account = (await _accounts.GetAccountsAsync([signedIn], cancellationToken)).SingleOrDefault();
            if (account is null || !string.Equals(account.Email, invitation.Email, StringComparison.OrdinalIgnoreCase))
            {
                return new AcceptInvitationResult(AcceptInvitationStatus.EmailMismatch);
            }

            userId = signedIn;
        }
        else
        {
            if (await _accounts.FindUserIdByEmailAsync(invitation.Email, cancellationToken) is not null)
            {
                return new AcceptInvitationResult(AcceptInvitationStatus.AccountExists);
            }

            var errors = ValidateNewAccount(request);
            if (errors.Count > 0)
            {
                return new AcceptInvitationResult(AcceptInvitationStatus.Invalid, errors);
            }

            // The link reached this address, so it counts as confirmed.
            var created = await _accounts.CreateAccountAsync(
                new NewAccount(invitation.Email, request.DisplayName!.Trim(), request.Password!, EmailConfirmed: true,
                    PrivacyPolicy.CurrentVersion),
                cancellationToken);

            if (created.UserId is not { } newUserId)
            {
                return new AcceptInvitationResult(AcceptInvitationStatus.Invalid, new Dictionary<string, string[]>
                {
                    ["password"] = created.Errors.ToArray(),
                });
            }

            userId = newUserId;
            accountCreated = true;
        }

        if (await unitOfWork.Memberships.FindAsync(invitation.OrganizationId, userId, cancellationToken) is null)
        {
            unitOfWork.Memberships.Add(new MembershipEntity
            {
                OrganizationId = invitation.OrganizationId,
                UserId = userId,
                Role = invitation.Role,
            });
        }

        invitation.AcceptedAt = _timeProvider.GetUtcNow().UtcDateTime;

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Accepted twice at the same moment: the unique membership index stops the second one.
            await DeleteCreatedAccountAsync();
            return new AcceptInvitationResult(AcceptInvitationStatus.Gone);
        }
        catch
        {
            await DeleteCreatedAccountAsync();
            throw;
        }

        LogAccepted(invitation.Id, userId);
        return new AcceptInvitationResult(
            AcceptInvitationStatus.Success,
            Session: await _sessions.SignInAsync(userId, cancellationToken));

        // An account made for this invitation but left without its membership would block the address (409) for good.
        async Task DeleteCreatedAccountAsync()
        {
            if (accountCreated)
            {
                await _accounts.DeleteAccountAsync(userId, CancellationToken.None);
            }
        }
    }

    private bool IsUsable(InvitationEntity invitation) =>
        invitation.AcceptedAt is null
        && invitation.RevokedAt is null
        && invitation.ExpiresAt > _timeProvider.GetUtcNow().UtcDateTime;

    private static InvitationInfo ToInfo(InvitationEntity invitation, DateTimeOffset now) => new(
        invitation.Id,
        invitation.Email,
        invitation.Role,
        new DateTimeOffset(invitation.ExpiresAt, TimeSpan.Zero),
        invitation.ExpiresAt > now.UtcDateTime ? InvitationState.Pending : InvitationState.Expired);

    private static Dictionary<string, string[]> ValidateNewAccount(AcceptInvitationRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.DisplayName) || request.DisplayName.Trim().Length > UserEntity.DisplayNameMaxLength)
        {
            errors["displayName"] = [$"Required, at most {UserEntity.DisplayNameMaxLength} characters."];
        }

        if (string.IsNullOrEmpty(request.Password))
        {
            errors["password"] = ["Required."];
        }

        if (!request.PrivacyAccepted)
        {
            errors["privacyAccepted"] = ["The privacy notice must be accepted."];
        }

        return errors;
    }

    private static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    [LoggerMessage(Level = LogLevel.Information, Message = "Invitation {InvitationId} accepted by user {UserId}.")]
    private partial void LogAccepted(long invitationId, long userId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Mail for invitation {InvitationId} could not be delivered.")]
    private partial void LogMailFailed(long invitationId);
}
