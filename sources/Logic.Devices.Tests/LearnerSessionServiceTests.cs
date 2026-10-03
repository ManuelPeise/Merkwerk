using System.Globalization;
using Logic.Authentication;
using Logic.Devices.Sessions;
using Logic.Devices.Tests.Infrastructure;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Logic.Devices.Tests;

/// <summary>LP-106: children sign in on paired devices – bound to device and child, 8 hours, never across families.</summary>
[Collection(DevicesDatabaseCollection.Name)]
public sealed class LearnerSessionServiceTests(DevicesDatabaseFixture database)
{
    private readonly DevicesTestContext _context = DevicesTestContext.Create(database.ConnectionString);

    [Fact]
    public async Task SignInAsync_ChildOfTheFamily_IssuesLearnerTokenBoundToDeviceAndChild()
    {
        // Arrange
        var family = await _context.CreateFamilyAsync();
        var learnerId = await _context.AddLearnerAsync(family, "Mia", "owl");
        var deviceToken = await _context.PairDeviceAsync(family);

        // Act
        var result = await SignInAsync(deviceToken, learnerId);

        // Assert
        Assert.Equal(LearnerSignInStatus.Success, result.Status);
        var session = result.Session!;
        Assert.Equal("Mia", session.Name);
        Assert.Equal(AuthRoles.Learner, session.Role);
        Assert.Equal("owl", session.AvatarId);
        Assert.Equal(_context.Time.GetUtcNow().Add(DeviceLifetimes.LearnerSession), session.RefreshTokenExpiresAt);

        var token = new JsonWebToken(session.AccessToken);
        var deviceId = await DeviceIdAsync(family);
        Assert.Equal(AuthRoles.Learner, token.GetClaim(AuthClaims.Role).Value);
        Assert.Equal(learnerId.ToString(CultureInfo.InvariantCulture), token.GetClaim(AuthClaims.LearnerId).Value);
        Assert.Equal(learnerId.ToString(CultureInfo.InvariantCulture), token.GetClaim(AuthClaims.Subject).Value);
        Assert.Equal(deviceId.ToString(CultureInfo.InvariantCulture), token.GetClaim(AuthClaims.DeviceId).Value);
        Assert.Equal(family.OrganizationId.ToString(CultureInfo.InvariantCulture), token.GetClaim(AuthClaims.OrganizationId).Value);
    }

    [Fact]
    public async Task SignInAsync_ChildOfOtherFamily_LearnerNotFound()
    {
        // Arrange: the tablet of family A, a child id of family B.
        var familyA = await _context.CreateFamilyAsync();
        var familyB = await _context.CreateFamilyAsync();
        var childOfB = await _context.AddLearnerAsync(familyB);
        var deviceToken = await _context.PairDeviceAsync(familyA);

        // Act
        var result = await SignInAsync(deviceToken, childOfB);

        // Assert
        Assert.Equal(LearnerSignInStatus.LearnerNotFound, result.Status);
        Assert.Null(result.Session);
    }

    [Fact]
    public async Task SignInAsync_UnknownDevice_DeviceNotPaired()
    {
        var family = await _context.CreateFamilyAsync();
        var learnerId = await _context.AddLearnerAsync(family);
        _context.ActAsDevice();

        var result = await SignInAsync("not-a-device-token", learnerId);

        Assert.Equal(LearnerSignInStatus.DeviceNotPaired, result.Status);
    }

    [Fact]
    public async Task SignInAsync_RevokedDevice_DeviceNotPairedAndRefreshEnds()
    {
        // Arrange: a child is signed in, then the parents unpair the tablet.
        var family = await _context.CreateFamilyAsync();
        var learnerId = await _context.AddLearnerAsync(family);
        var deviceToken = await _context.PairDeviceAsync(family);
        var session = (await SignInAsync(deviceToken, learnerId)).Session!;
        var deviceId = await DeviceIdAsync(family);
        _context.ActAs(family);
        Assert.True(await _context.RunAsync<IDeviceService, bool>(s =>
            s.RevokeAsync(family.OrganizationId, family.OwnerUserId, deviceId, default)));

        // Act
        var signIn = await SignInAsync(deviceToken, learnerId);
        var refreshed = await RefreshAsync(session.RefreshToken);

        // Assert
        Assert.Equal(LearnerSignInStatus.DeviceNotPaired, signIn.Status);
        Assert.Null(refreshed);
    }

    [Fact]
    public async Task RefreshAsync_WithinEightHours_NewAccessTokenNeverBeyondSessionEnd()
    {
        // Arrange
        var family = await _context.CreateFamilyAsync();
        var learnerId = await _context.AddLearnerAsync(family);
        var deviceToken = await _context.PairDeviceAsync(family);
        var session = (await SignInAsync(deviceToken, learnerId)).Session!;

        // Act: 5 minutes before the end – a normal access token would live 15 minutes.
        _context.Time.Advance(DeviceLifetimes.LearnerSession - TimeSpan.FromMinutes(5));
        var refreshed = await RefreshAsync(session.RefreshToken);

        // Assert
        Assert.NotNull(refreshed);
        Assert.Equal(AuthRoles.Learner, refreshed.Role);
        Assert.Equal(session.RefreshTokenExpiresAt, refreshed.AccessTokenExpiresAt);
    }

    [Fact]
    public async Task RefreshAsync_AfterEightHours_SessionHasEnded()
    {
        var family = await _context.CreateFamilyAsync();
        var learnerId = await _context.AddLearnerAsync(family);
        var deviceToken = await _context.PairDeviceAsync(family);
        var session = (await SignInAsync(deviceToken, learnerId)).Session!;

        _context.Time.Advance(DeviceLifetimes.LearnerSession + TimeSpan.FromSeconds(1));
        var refreshed = await RefreshAsync(session.RefreshToken);

        Assert.Null(refreshed);
    }

    [Fact]
    public async Task SignOutAsync_SwitchChild_EndsTheSessionButNotTheDevice()
    {
        // Arrange
        var family = await _context.CreateFamilyAsync();
        var learnerId = await _context.AddLearnerAsync(family);
        var deviceToken = await _context.PairDeviceAsync(family);
        var session = (await SignInAsync(deviceToken, learnerId)).Session!;

        // Act
        await _context.RunAsync<ILearnerSessionService, bool>(async s =>
        {
            await s.SignOutAsync(session.RefreshToken, default);
            return true;
        });

        // Assert
        Assert.Null(await RefreshAsync(session.RefreshToken));
        Assert.Equal(LearnerSignInStatus.Success, (await SignInAsync(deviceToken, learnerId)).Status);
    }

    [Fact]
    public async Task SignInAsync_UsedDevice_StaysPairedBeyondTheFirst180Days()
    {
        // Arrange: paired, used again after 100 days.
        var family = await _context.CreateFamilyAsync();
        var learnerId = await _context.AddLearnerAsync(family);
        var deviceToken = await _context.PairDeviceAsync(family);
        _context.Time.Advance(TimeSpan.FromDays(100));
        var signedInAt = _context.Time.GetUtcNow();
        var signIn = await SignInAsync(deviceToken, learnerId);

        // Act: 181 days after pairing.
        _context.Time.Advance(TimeSpan.FromDays(81));
        var status = await _context.RunAsync<IDeviceService, DeviceStatus?>(s => s.GetStatusAsync(deviceToken, default));

        // Assert
        Assert.NotNull(status);
        Assert.Equal(signedInAt.Add(DeviceLifetimes.DeviceToken), signIn.DeviceTokenExpiresAt);
    }

    private Task<LearnerSignInResult> SignInAsync(string deviceToken, long learnerId)
    {
        _context.ActAsDevice();
        return _context.RunAsync<ILearnerSessionService, LearnerSignInResult>(s => s.SignInAsync(deviceToken, learnerId, default));
    }

    private Task<AuthSession?> RefreshAsync(string refreshToken)
    {
        _context.ActAsDevice();
        return _context.RunAsync<ILearnerSessionService, AuthSession?>(s => s.RefreshAsync(refreshToken, default));
    }

    private async Task<long> DeviceIdAsync(Family family)
    {
        _context.ActAs(family);
        var devices = await _context.RunAsync<IDeviceService, IReadOnlyList<DeviceInfo>?>(s =>
            s.ListAsync(family.OrganizationId, family.OwnerUserId, default));
        return Assert.Single(devices!).Id;
    }
}
