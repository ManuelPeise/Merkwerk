using Data.Accessor.Abstractions;
using Data.Database.Entities.Devices;
using Data.Database.Entities.Learners;
using Logic.Authentication;
using Logic.Shared.Interfaces;
using Shared.Enums;
using Shared.Models.Authentication;
using Shared.Models.Devices;

namespace Logic.Devices.Sessions;

internal sealed class LearnerSessionService : ILearnerSessionService
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly DeviceService _devices;
    private readonly TokenService _tokenService;
    private readonly TimeProvider _timeProvider;

    public LearnerSessionService(
        IUnitOfWorkFactory unitOfWorkFactory,
        DeviceService devices,
        TokenService tokenService,
        TimeProvider timeProvider)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _devices = devices;
        _tokenService = tokenService;
        _timeProvider = timeProvider;
    }

    public async Task<LearnerSignInResult> SignInAsync(string deviceToken, long learnerId, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var device = await _devices.FindActiveDeviceAsync(unitOfWork, deviceToken, cancellationToken);

        if (device is null)
        {
            return new LearnerSignInResult(LearnerSignInStatus.DeviceNotPaired);
        }

        // Explicitly within the device's family – another family's child id is "not found".
        var learner = await unitOfWork.Learners.FindForDeviceAsync(device.OrganizationId, learnerId, cancellationToken);

        if (learner is null)
        {
            return new LearnerSignInResult(LearnerSignInStatus.LearnerNotFound);
        }

        var deviceExpiresAt = now.Add(DeviceLifetimes.DeviceToken);
        device.LastSeenAt = now.UtcDateTime;
        device.ExpiresAt = deviceExpiresAt.UtcDateTime;

        var refreshToken = DeviceTokens.CreateToken();
        var sessionEndsAt = now.Add(DeviceLifetimes.LearnerSession);
        unitOfWork.LearnerSessions.Add(new LearnerSessionEntity
        {
            OrganizationId = device.OrganizationId,
            DeviceId = device.Id,
            LearnerId = learner.Id,
            TokenHash = DeviceTokens.Hash(refreshToken),
            ExpiresAt = sessionEndsAt.UtcDateTime,
        });
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LearnerSignInResult(
            LearnerSignInStatus.Success,
            CreateSession(learner, device, refreshToken, sessionEndsAt),
            deviceExpiresAt);
    }

    public async Task<AuthSession?> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return null;
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var session = await unitOfWork.LearnerSessions.FindByTokenHashAsync(DeviceTokens.Hash(refreshToken), cancellationToken);

        if (session is not { RevokedAt: null, Device: { RevokedAt: null } device, Learner: { } learner }
            || session.ExpiresAt <= now
            || device.ExpiresAt <= now)
        {
            return null;
        }

        // Name and avatar are read again, so changes in the parents' area show up after at most one access token.
        return CreateSession(learner, device, refreshToken, AsUtc(session.ExpiresAt));
    }

    public async Task SignOutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return;
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var session = await unitOfWork.LearnerSessions.FindByTokenHashAsync(DeviceTokens.Hash(refreshToken), cancellationToken);

        if (session is { RevokedAt: null })
        {
            session.RevokedAt = _timeProvider.GetUtcNow().UtcDateTime;
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private AuthSession CreateSession(LearnerEntity learner, DeviceEntity device, string refreshToken, DateTimeOffset sessionEndsAt)
    {
        var (accessToken, accessExpiresAt) = _tokenService.CreateLearnerAccessToken(
            learner.Id, learner.DisplayName, learner.AvatarId, device.OrganizationId, device.Id, sessionEndsAt);

        return new AuthSession(
            learner.DisplayName,
            AuthRoles.Learner,
            MustChangePassword: false,
            accessToken,
            accessExpiresAt,
            refreshToken,
            sessionEndsAt,
            learner.AvatarId);
    }

    private static DateTimeOffset AsUtc(DateTime value) =>
        new(DateTime.SpecifyKind(value, DateTimeKind.Utc), TimeSpan.Zero);
}
