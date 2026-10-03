using Data.Accessor.Abstractions;
using Data.Database.Entities.Identity;
using Data.Database.Entities.Organizations;
using Logic.Authentication;
using Logic.Authentication.Accounts;
using Microsoft.Extensions.Logging;

namespace Logic.Organizations.Setup;

internal sealed partial class SetupService(
    IUnitOfWorkFactory unitOfWorkFactory,
    IAccountService accounts,
    IAuthSessionService sessions,
    SetupLock setupLock,
    ILogger<SetupService> logger) : ISetupService
{
    public async Task<bool> IsSetupRequiredAsync(CancellationToken cancellationToken)
    {
        await using var unitOfWork = unitOfWorkFactory.Create();
        return !await unitOfWork.Organizations.AnyAsync(cancellationToken);
    }

    public async Task<SetupResult> InitializeAsync(SetupRequest request, CancellationToken cancellationToken)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
        {
            return new SetupResult(SetupStatus.Invalid, errors);
        }

        await setupLock.Semaphore.WaitAsync(cancellationToken);
        try
        {
            if (!await IsSetupRequiredAsync(cancellationToken))
            {
                return new SetupResult(SetupStatus.AlreadyDone, Empty);
            }

            var account = await accounts.CreateAccountAsync(
                new NewAccount(request.Email.Trim(), request.DisplayName.Trim(), request.Password, EmailConfirmed: true,
                    PrivacyPolicy.CurrentVersion),
                cancellationToken);

            if (account.UserId is not { } userId)
            {
                return new SetupResult(SetupStatus.Invalid, new Dictionary<string, string[]>
                {
                    ["password"] = account.Errors.ToArray(),
                });
            }

            try
            {
                await CreateFamilyAsync(request.FamilyName.Trim(), userId, cancellationToken);
            }
            catch
            {
                // Without a family the account would be orphaned and block a second attempt with the same address.
                await accounts.DeleteAccountAsync(userId, CancellationToken.None);
                throw;
            }

            LogInitialized(userId);
            return new SetupResult(SetupStatus.Success, Empty, await sessions.SignInAsync(userId, cancellationToken));
        }
        finally
        {
            setupLock.Semaphore.Release();
        }
    }

    private static readonly IReadOnlyDictionary<string, string[]> Empty = new Dictionary<string, string[]>();

    private async Task CreateFamilyAsync(string familyName, long ownerUserId, CancellationToken cancellationToken)
    {
        // One save = one transaction: a family saved without its owner would block the setup for good.
        await using var unitOfWork = unitOfWorkFactory.Create();
        var organization = new Organization { Name = familyName };
        unitOfWork.Organizations.Add(organization);
        unitOfWork.Memberships.Add(new Membership
        {
            Organization = organization,
            UserId = ownerUserId,
            Role = OrganizationRole.OrgAdmin,
            IsOwner = true,
        });
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static Dictionary<string, string[]> Validate(SetupRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.FamilyName) || request.FamilyName.Trim().Length > Organization.NameMaxLength)
        {
            errors["familyName"] = [$"Required, at most {Organization.NameMaxLength} characters."];
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName) || request.DisplayName.Trim().Length > User.DisplayNameMaxLength)
        {
            errors["displayName"] = [$"Required, at most {User.DisplayNameMaxLength} characters."];
        }

        if (!request.PrivacyAccepted)
        {
            errors["privacyAccepted"] = ["The privacy notice must be accepted."];
        }

        return errors;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "First-run setup done, owner is user {UserId}.")]
    private partial void LogInitialized(long userId);
}
