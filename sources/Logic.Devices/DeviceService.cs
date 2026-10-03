using Data.Accessor.Abstractions;
using Data.Database.Entities.Devices;
using Logic.Devices.Pairing;
using Logic.Notifications.Links;
using Microsoft.EntityFrameworkCore;

namespace Logic.Devices;

internal sealed class DeviceService(
    IUnitOfWorkFactory unitOfWorkFactory,
    IPublicLinkBuilder links,
    TimeProvider timeProvider) : IDeviceService
{
    /// <summary>Client route that pairs with <c>?code=</c> (QR code, LP-125).</summary>
    public const string PairingPath = "/practice/pair";

    public const string DefaultDeviceName = "Tablet";

    private const int MaxCodeAttempts = 5;

    public async Task<PairingCodeInfo?> CreatePairingCodeAsync(
        long organizationId,
        long actingUserId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await TryCreatePairingCodeAsync(organizationId, actingUserId, cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // A device used the open code while we replaced it; nothing of ours was written – try once more.
            return await TryCreatePairingCodeAsync(organizationId, actingUserId, cancellationToken);
        }
    }

    public async Task<PairResult> PairAsync(string code, string deviceName, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        await using var unitOfWork = unitOfWorkFactory.Create();
        var pairingCode = await unitOfWork.PairingCodes.FindUsableByHashAsync(
            DeviceTokens.Hash(code.Trim()), now.UtcDateTime, cancellationToken);

        if (pairingCode is null)
        {
            return new PairResult(PairStatus.InvalidCode);
        }

        var organization = await unitOfWork.Organizations.GetByIdAsync(pairingCode.OrganizationId, cancellationToken);
        var token = DeviceTokens.CreateToken();
        var expiresAt = now.Add(DeviceLifetimes.DeviceToken);

        pairingCode.UsedAt = now.UtcDateTime;
        unitOfWork.Devices.Add(new Device
        {
            OrganizationId = pairingCode.OrganizationId,
            Name = NormalizeName(deviceName),
            TokenHash = DeviceTokens.Hash(token),
            ExpiresAt = expiresAt.UtcDateTime,
            PairedByUserId = pairingCode.CreatedByUserId,
        });

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another device used the same code a moment earlier (UsedAt is a concurrency token).
            return new PairResult(PairStatus.InvalidCode);
        }

        return new PairResult(PairStatus.Success, token, expiresAt, organization?.Name ?? string.Empty);
    }

    public async Task<DeviceStatus?> GetStatusAsync(string deviceToken, CancellationToken cancellationToken)
    {
        await using var unitOfWork = unitOfWorkFactory.Create();
        var device = await FindActiveDeviceAsync(unitOfWork, deviceToken, cancellationToken);

        if (device is null)
        {
            return null;
        }

        var organization = await unitOfWork.Organizations.GetByIdAsync(device.OrganizationId, cancellationToken);
        return new DeviceStatus(device.Id, organization?.Name ?? string.Empty);
    }

    public async Task<IReadOnlyList<DeviceProfile>?> ListProfilesAsync(string deviceToken, CancellationToken cancellationToken)
    {
        await using var unitOfWork = unitOfWorkFactory.Create();
        var device = await FindActiveDeviceAsync(unitOfWork, deviceToken, cancellationToken);

        if (device is null)
        {
            return null;
        }

        var learners = await unitOfWork.Learners.ListForDeviceAsync(device.OrganizationId, cancellationToken);
        return learners.Select(l => new DeviceProfile(l.Id, l.DisplayName, l.AvatarId)).ToList();
    }

    public async Task<IReadOnlyList<DeviceInfo>?> ListAsync(
        long organizationId,
        long actingUserId,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        await using var unitOfWork = unitOfWorkFactory.Create();
        if (await unitOfWork.Memberships.FindAsync(organizationId, actingUserId, cancellationToken) is null)
        {
            return null;
        }

        return await unitOfWork.Devices.Query()
            .Where(d => d.OrganizationId == organizationId && d.RevokedAt == null && d.ExpiresAt > now)
            .OrderBy(d => d.Name)
            .ThenBy(d => d.Id)
            .Select(d => new DeviceInfo(d.Id, d.Name, d.CreatedAt, d.LastSeenAt, d.ExpiresAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> RevokeAsync(
        long organizationId,
        long actingUserId,
        long deviceId,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        await using var unitOfWork = unitOfWorkFactory.Create();
        if (await unitOfWork.Memberships.FindAsync(organizationId, actingUserId, cancellationToken) is null)
        {
            return false;
        }

        var device = await unitOfWork.Devices.GetByIdAsync(deviceId, cancellationToken);

        // Explicit tenant check in addition to the query filter (ADR 007).
        if (device is null || device.OrganizationId != organizationId || device.RevokedAt is not null)
        {
            return false;
        }

        device.RevokedAt = now;

        var sessions = await unitOfWork.LearnerSessions.QueryTracked()
            .Where(s => s.DeviceId == deviceId && s.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var session in sessions)
        {
            session.RevokedAt = now;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>Device of the token, if it is neither unpaired nor expired (tracked).</summary>
    internal async Task<Device?> FindActiveDeviceAsync(
        IUnitOfWork unitOfWork,
        string deviceToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(deviceToken))
        {
            return null;
        }

        var device = await unitOfWork.Devices.FindByTokenHashAsync(DeviceTokens.Hash(deviceToken), cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        return device is { RevokedAt: null } && device.ExpiresAt > now ? device : null;
    }

    private async Task<PairingCodeInfo?> TryCreatePairingCodeAsync(
        long organizationId,
        long actingUserId,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        await using var unitOfWork = unitOfWorkFactory.Create();
        if (await unitOfWork.Memberships.FindAsync(organizationId, actingUserId, cancellationToken) is null)
        {
            return null;
        }

        // Only one open code per family: fewer valid codes, less to guess.
        var open = await unitOfWork.PairingCodes.QueryTracked()
            .Where(c => c.OrganizationId == organizationId && c.UsedAt == null && c.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var replaced in open)
        {
            replaced.RevokedAt = now.UtcDateTime;
        }

        var code = await CreateUnusedCodeAsync(unitOfWork, now.UtcDateTime, cancellationToken);
        var expiresAt = now.Add(DeviceLifetimes.PairingCode);

        unitOfWork.PairingCodes.Add(new PairingCode
        {
            OrganizationId = organizationId,
            CodeHash = DeviceTokens.Hash(code),
            ExpiresAt = expiresAt.UtcDateTime,
            CreatedByUserId = actingUserId,
        });
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new PairingCodeInfo(code, expiresAt, links.Build(PairingPath, ("code", code)));
    }

    /// <summary>Six digits that no other family's open code uses right now.</summary>
    private static async Task<string> CreateUnusedCodeAsync(IUnitOfWork unitOfWork, DateTime now, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxCodeAttempts; attempt++)
        {
            var code = DeviceTokens.CreatePairingCode();

            if (await unitOfWork.PairingCodes.FindUsableByHashAsync(DeviceTokens.Hash(code), now, cancellationToken) is null)
            {
                return code;
            }
        }

        throw new InvalidOperationException("No free pairing code found.");
    }

    private static string NormalizeName(string name)
    {
        var trimmed = name.Trim();

        if (trimmed.Length == 0)
        {
            return DefaultDeviceName;
        }

        return trimmed.Length > Device.NameMaxLength ? trimmed[..Device.NameMaxLength] : trimmed;
    }
}
