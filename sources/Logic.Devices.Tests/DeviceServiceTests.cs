using Data.Accessor.Abstractions;
using Logic.Devices.Tests.Infrastructure;
using Logic.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Enums;
using Shared.Models.Devices;

namespace Logic.Devices.Tests;

/// <summary>LP-106: pairing codes, pairing and managing devices – never across families.</summary>
[Collection(DevicesDatabaseCollection.Name)]
public sealed class DeviceServiceTests(DevicesDatabaseFixture database)
{
    private readonly DevicesTestContext _context = DevicesTestContext.Create(database.ConnectionString);

    [Fact]
    public async Task CreatePairingCodeAsync_Member_ReturnsSixDigitsAndPairingUrl()
    {
        // Arrange
        var family = await _context.CreateFamilyAsync();
        var memberId = await _context.CreateAccountAsync("Oma");
        await _context.AddMemberAsync(family, memberId, OrganizationRole.Member);
        _context.ActAs(family);

        // Act
        var code = await _context.RunAsync<IDeviceService, PairingCodeInfo?>(s =>
            s.CreatePairingCodeAsync(family.OrganizationId, memberId, default));

        // Assert
        Assert.NotNull(code);
        Assert.Matches("^[0-9]{6}$", code.Code);
        Assert.Equal($"http://localhost:65350/practice/pair?code={code.Code}", code.PairingUrl);
        Assert.Equal(_context.Time.GetUtcNow().Add(DeviceLifetimes.PairingCode), code.ExpiresAt);
    }

    [Fact]
    public async Task CreatePairingCodeAsync_AdultOfOtherFamily_ReturnsNull()
    {
        var family = await _context.CreateFamilyAsync();
        var otherFamily = await _context.CreateFamilyAsync();
        _context.ActAs(family);

        var code = await _context.RunAsync<IDeviceService, PairingCodeInfo?>(s =>
            s.CreatePairingCodeAsync(family.OrganizationId, otherFamily.OwnerUserId, default));

        Assert.Null(code);
    }

    [Fact]
    public async Task PairAsync_ValidCode_PairsDeviceWithFamily()
    {
        // Arrange
        var family = await _context.CreateFamilyAsync();
        var code = await _context.CreatePairingCodeAsync(family);
        _context.ActAsDevice();

        // Act
        var result = await PairAsync(code.Code, "  Küchen-Tablet ");
        var status = await _context.RunAsync<IDeviceService, DeviceStatus?>(s => s.GetStatusAsync(result.DeviceToken!, default));
        var devices = await ListAsync(family);

        // Assert
        Assert.Equal(PairStatus.Success, result.Status);
        Assert.Equal(family.Name, result.FamilyName);
        Assert.Equal(_context.Time.GetUtcNow().Add(DeviceLifetimes.DeviceToken), result.DeviceTokenExpiresAt);
        Assert.Equal(family.Name, status!.FamilyName);
        Assert.Equal("Küchen-Tablet", Assert.Single(devices).Name);
    }

    [Fact]
    public async Task PairAsync_CodeUsedTwice_SecondIsInvalid()
    {
        var family = await _context.CreateFamilyAsync();
        var code = await _context.CreatePairingCodeAsync(family);
        _context.ActAsDevice();

        var first = await PairAsync(code.Code);
        var second = await PairAsync(code.Code);

        Assert.Equal(PairStatus.Success, first.Status);
        Assert.Equal(PairStatus.InvalidCode, second.Status);
    }

    [Fact]
    public async Task PairAsync_CodeOlderThanTenMinutes_IsInvalid()
    {
        var family = await _context.CreateFamilyAsync();
        var code = await _context.CreatePairingCodeAsync(family);
        _context.ActAsDevice();
        _context.Time.Advance(DeviceLifetimes.PairingCode + TimeSpan.FromSeconds(1));

        var result = await PairAsync(code.Code);

        Assert.Equal(PairStatus.InvalidCode, result.Status);
    }

    [Fact]
    public async Task CreatePairingCodeAsync_SecondCode_ReplacesTheOpenOne()
    {
        var family = await _context.CreateFamilyAsync();
        var first = await _context.CreatePairingCodeAsync(family);
        var second = await _context.CreatePairingCodeAsync(family);
        _context.ActAsDevice();

        var withFirst = await PairAsync(first.Code);
        var withSecond = await PairAsync(second.Code);

        Assert.Equal(PairStatus.InvalidCode, withFirst.Status);
        Assert.Equal(PairStatus.Success, withSecond.Status);
    }

    [Fact]
    public async Task PairAsync_CodeReplacedWhilePairing_CannotMarkTheOldCodeUsed()
    {
        // Arrange: the device has read the code as usable (first half of PairAsync) ...
        var family = await _context.CreateFamilyAsync();
        var code = await _context.CreatePairingCodeAsync(family);
        _context.ActAsDevice();
        await using var scope = _context.Services.CreateAsyncScope();
        await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>().Create();
        var read = await unitOfWork.PairingCodes.FindUsableByHashAsync(
            DeviceTokens.Hash(code.Code), _context.Time.GetUtcNow().UtcDateTime);

        // ... and a parent creates a new code at the same moment, which revokes the old one.
        await _context.CreatePairingCodeAsync(family);

        // Act: the device marks its (now revoked) code used.
        read!.UsedAt = _context.Time.GetUtcNow().UtcDateTime;

        // Assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => unitOfWork.SaveChangesAsync());
    }

    [Fact]
    public async Task ListProfilesAsync_PairedDevice_ReturnsOnlyChildrenOfItsFamily()
    {
        // Arrange
        var family = await _context.CreateFamilyAsync();
        var otherFamily = await _context.CreateFamilyAsync();
        var mia = await _context.AddLearnerAsync(family, "Mia", "fox");
        await _context.AddLearnerAsync(otherFamily, "Tom", "owl");
        var deviceToken = await _context.PairDeviceAsync(family);

        // Act: anonymous device request, no organization claim.
        _context.ActAsDevice();
        var profiles = await ListProfilesAsync(deviceToken);

        // Assert
        Assert.Equal(new DeviceProfile(mia, "Mia", "fox"), Assert.Single(profiles!));
    }

    [Fact]
    public async Task ListProfilesAsync_UnknownToken_ReturnsNull()
    {
        _context.ActAsDevice();

        var profiles = await ListProfilesAsync("not-a-device-token");

        Assert.Null(profiles);
    }

    [Fact]
    public async Task RevokeAsync_Member_UnpairsTheDevice()
    {
        // Arrange
        var family = await _context.CreateFamilyAsync();
        var deviceToken = await _context.PairDeviceAsync(family);
        var deviceId = Assert.Single(await ListAsync(family)).Id;

        // Act
        var revoked = await RevokeAsync(family, family.OwnerUserId, deviceId);

        // Assert
        _context.ActAsDevice();
        Assert.True(revoked);
        Assert.Null(await _context.RunAsync<IDeviceService, DeviceStatus?>(s => s.GetStatusAsync(deviceToken, default)));
        Assert.Null(await ListProfilesAsync(deviceToken));
        Assert.Empty(await ListAsync(family));
    }

    [Fact]
    public async Task RevokeAsync_DeviceOfOtherFamily_NotFoundAndStaysPaired()
    {
        // Arrange: a device of family A; the admin of family B guesses its id.
        var familyA = await _context.CreateFamilyAsync();
        var deviceToken = await _context.PairDeviceAsync(familyA);
        var deviceId = Assert.Single(await ListAsync(familyA)).Id;
        var familyB = await _context.CreateFamilyAsync();

        // Act
        var revoked = await RevokeAsync(familyB, familyB.OwnerUserId, deviceId);

        // Assert
        _context.ActAsDevice();
        Assert.False(revoked);
        Assert.NotNull(await _context.RunAsync<IDeviceService, DeviceStatus?>(s => s.GetStatusAsync(deviceToken, default)));
    }

    [Fact]
    public async Task ListAsync_AdultOfOtherFamily_ReturnsNull()
    {
        var family = await _context.CreateFamilyAsync();
        var otherFamily = await _context.CreateFamilyAsync();
        _context.ActAs(family);

        var devices = await _context.RunAsync<IDeviceService, IReadOnlyList<DeviceInfo>?>(s =>
            s.ListAsync(family.OrganizationId, otherFamily.OwnerUserId, default));

        Assert.Null(devices);
    }

    [Fact]
    public async Task GetStatusAsync_UnusedFor180Days_IsNotPaired()
    {
        var family = await _context.CreateFamilyAsync();
        var deviceToken = await _context.PairDeviceAsync(family);
        _context.Time.Advance(DeviceLifetimes.DeviceToken + TimeSpan.FromMinutes(1));

        var status = await _context.RunAsync<IDeviceService, DeviceStatus?>(s => s.GetStatusAsync(deviceToken, default));

        Assert.Null(status);
    }

    private Task<PairResult> PairAsync(string code, string deviceName = "Tablet") =>
        _context.RunAsync<IDeviceService, PairResult>(s => s.PairAsync(code, deviceName, default));

    private Task<IReadOnlyList<DeviceProfile>?> ListProfilesAsync(string deviceToken) =>
        _context.RunAsync<IDeviceService, IReadOnlyList<DeviceProfile>?>(s => s.ListProfilesAsync(deviceToken, default));

    private async Task<IReadOnlyList<DeviceInfo>> ListAsync(Family family)
    {
        _context.ActAs(family);
        return (await _context.RunAsync<IDeviceService, IReadOnlyList<DeviceInfo>?>(s =>
            s.ListAsync(family.OrganizationId, family.OwnerUserId, default)))!;
    }

    private Task<bool> RevokeAsync(Family family, long actingUserId, long deviceId)
    {
        _context.ActAs(family);
        return _context.RunAsync<IDeviceService, bool>(s => s.RevokeAsync(family.OrganizationId, actingUserId, deviceId, default));
    }
}
