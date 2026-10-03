using Data.Accessor.Abstractions;
using Data.Accessor.DI;
using Data.Database;
using Data.Database.Abstractions;
using Data.Database.Entities.Learners;
using Data.Database.Entities.Organizations;
using Logic.Authentication;
using Logic.Authentication.Accounts;
using Logic.Authentication.DI;
using Logic.Devices.DI;
using Logic.Devices.Pairing;
using Logic.Notifications.DI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Web.Core.Bundels;

namespace Logic.Devices.Tests.Infrastructure;

/// <summary>
/// The real registrations against the test database, with a manual clock and a settable organization.
/// Every call runs in its own scope, like one HTTP request.
/// </summary>
public sealed class DevicesTestContext
{
    public const string Password = "correct-horse-battery";
    public const string SigningKey = "test-signing-key-with-at-least-32-bytes!";

    private DevicesTestContext(ServiceProvider services)
    {
        Services = services;
    }

    public ServiceProvider Services { get; }

    /// <summary>Starts on a whole second: MySQL keeps microseconds, so times read back compare equal.</summary>
    public ManualTimeProvider Time { get; } = new(WholeSecondNow());

    public TestCurrentUser CurrentUser { get; } = new();

    public static DevicesTestContext Create(string connectionString)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = connectionString,
                ["Auth:Jwt:SigningKey"] = SigningKey,
                ["Mail:Host"] = "localhost",
                ["Mail:FromAddress"] = "noreply@merkwerk.local",
                ["App:PublicBaseUrl"] = "http://localhost:65350",
            })
            .Build();

        var services = new ServiceCollection();
        DevicesTestContext? context = null;

        services.AddLogging();
        services.AddSingleton<TimeProvider>(_ => context!.Time);
        services.AddScoped<ICurrentUser>(_ => context!.CurrentUser);
        services.AddMerkwerkDataAccess(configuration);
        services.AddMerkwerkNotifications(configuration);
        services.AddMerkwerkIdentity(configuration);
        services.AddMerkwerkAuthentication(configuration);
        services.AddMerkwerkDevices();

        context = new DevicesTestContext(services.BuildServiceProvider(validateScopes: true));
        return context;
    }

    public async Task<T> RunAsync<TService, T>(Func<TService, Task<T>> action)
        where TService : notnull
    {
        await using var scope = Services.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<TService>());
    }

    public async Task MigrateAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<MerkwerkDbContext>().Database.MigrateAsync();
    }

    /// <summary>Requests from now on come from an adult of the family (organization claim set).</summary>
    public void ActAs(Family family) => CurrentUser.OrganizationId = family.OrganizationId;

    /// <summary>Requests from now on are anonymous device requests (no organization claim).</summary>
    public void ActAsDevice() => CurrentUser.OrganizationId = null;

    /// <summary>A new adult account with a unique address (no membership).</summary>
    public async Task<long> CreateAccountAsync(string displayName = "Anna")
    {
        var email = $"{Guid.NewGuid():N}@example.org";
        var result = await RunAsync<IAccountService, AccountResult>(a => a.CreateAccountAsync(
            new NewAccount(email, displayName, Password, EmailConfirmed: true, PrivacyPolicyVersion: "test"), default));

        Assert.True(result.Succeeded, string.Join(", ", result.Errors));
        return result.UserId!.Value;
    }

    /// <summary>A family with its owner (admin), like after the first-run setup.</summary>
    public async Task<Family> CreateFamilyAsync(string? name = null)
    {
        name ??= $"Familie {Guid.NewGuid():N}"[..20];
        var ownerId = await CreateAccountAsync("Owner");

        await using var scope = Services.CreateAsyncScope();
        await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>().Create();
        var organization = new Organization { Name = name };
        unitOfWork.Organizations.Add(organization);
        await unitOfWork.SaveChangesAsync();
        unitOfWork.Memberships.Add(new Membership
        {
            OrganizationId = organization.Id,
            UserId = ownerId,
            Role = OrganizationRole.OrgAdmin,
            IsOwner = true,
        });
        await unitOfWork.SaveChangesAsync();

        return new Family(organization.Id, ownerId, name);
    }

    /// <summary>Adds an existing account to the family with the given role.</summary>
    public async Task AddMemberAsync(Family family, long userId, OrganizationRole role)
    {
        await using var scope = Services.CreateAsyncScope();
        await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>().Create();
        unitOfWork.Memberships.Add(new Membership { OrganizationId = family.OrganizationId, UserId = userId, Role = role });
        await unitOfWork.SaveChangesAsync();
    }

    /// <summary>A child profile in the family.</summary>
    public async Task<long> AddLearnerAsync(Family family, string displayName = "Mia", string avatarId = "fox")
    {
        await using var scope = Services.CreateAsyncScope();
        await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>().Create();
        var learner = new Learner { OrganizationId = family.OrganizationId, DisplayName = displayName, Grade = 2, AvatarId = avatarId };
        unitOfWork.Learners.Add(learner);
        await unitOfWork.SaveChangesAsync();
        return learner.Id;
    }

    /// <summary>New pairing code, created by the family's owner.</summary>
    public async Task<PairingCodeInfo> CreatePairingCodeAsync(Family family)
    {
        ActAs(family);
        var code = await RunAsync<IDeviceService, PairingCodeInfo?>(s =>
            s.CreatePairingCodeAsync(family.OrganizationId, family.OwnerUserId, default));
        return Assert.IsType<PairingCodeInfo>(code);
    }

    /// <summary>Pairs a new device with the family and returns its device token.</summary>
    public async Task<string> PairDeviceAsync(Family family, string deviceName = "Tablet")
    {
        var code = await CreatePairingCodeAsync(family);

        ActAsDevice();
        var result = await RunAsync<IDeviceService, PairResult>(s => s.PairAsync(code.Code, deviceName, default));

        Assert.Equal(PairStatus.Success, result.Status);
        return result.DeviceToken!;
    }

    private static DateTimeOffset WholeSecondNow()
    {
        var now = DateTimeOffset.UtcNow;
        return now.AddTicks(-(now.Ticks % TimeSpan.TicksPerSecond));
    }
}
