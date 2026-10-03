using Logic.Authentication;
using Logic.Organizations.Setup;
using Logic.Organizations.Tests.Infrastructure;

namespace Logic.Organizations.Tests;

/// <summary>LP-105: first-run setup on its own, empty database.</summary>
public sealed class SetupServiceTests(SetupDatabaseFixture database) : IClassFixture<SetupDatabaseFixture>
{
    private readonly OrganizationsTestContext _context = OrganizationsTestContext.Create(database.ConnectionString);

    [Fact]
    public async Task InitializeAsync_PrivacyNotAccepted_IsInvalid()
    {
        var result = await InitializeAsync("someone@example.org", privacyAccepted: false);

        Assert.Equal(SetupStatus.Invalid, result.Status);
        Assert.Contains("privacyAccepted", result.Errors.Keys);
    }

    [Fact]
    public async Task InitializeAsync_TwoParallelCallsOnFreshInstance_CreateExactlyOneOwner()
    {
        // Arrange
        Assert.True(await _context.RunAsync<ISetupService, bool>(s => s.IsSetupRequiredAsync(default)));

        // Act: two browsers press "set up" at the same moment.
        var results = await Task.WhenAll(
            InitializeAsync($"{Guid.NewGuid():N}@example.org"),
            InitializeAsync($"{Guid.NewGuid():N}@example.org"));
        var later = await InitializeAsync($"{Guid.NewGuid():N}@example.org");

        // Assert
        var success = Assert.Single(results, r => r.Status == SetupStatus.Success);
        Assert.Single(results, r => r.Status == SetupStatus.AlreadyDone);
        Assert.Equal(SetupStatus.AlreadyDone, later.Status);
        Assert.Equal(AuthRoles.OrgAdmin, success.Session!.Role);
        Assert.False(await _context.RunAsync<ISetupService, bool>(s => s.IsSetupRequiredAsync(default)));
    }

    private Task<SetupResult> InitializeAsync(string email, bool privacyAccepted = true) =>
        _context.RunAsync<ISetupService, SetupResult>(s => s.InitializeAsync(
            new SetupRequest("Familie Setup", "Anna", email, OrganizationsTestContext.Password, privacyAccepted),
            default));
}
