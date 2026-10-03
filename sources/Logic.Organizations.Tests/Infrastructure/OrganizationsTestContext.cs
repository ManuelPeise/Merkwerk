using Data.Accessor.Abstractions;
using Data.Accessor.DI;
using Data.Database;
using Data.Database.Abstractions;
using Data.Database.Entities.Organizations;
using Logic.Authentication;
using Logic.Authentication.Accounts;
using Logic.Authentication.DI;
using Logic.Notifications;
using Logic.Notifications.DI;
using Logic.Organizations.DI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Web.Core.Bundels;

namespace Logic.Organizations.Tests.Infrastructure;

/// <summary>
/// The real registrations against the test database, with a manual clock, a mail recorder and a settable organization.
/// Every call runs in its own scope, like one HTTP request.
/// </summary>
public sealed class OrganizationsTestContext
{
    public const string Password = "correct-horse-battery";

    private OrganizationsTestContext(ServiceProvider services)
    {
        Services = services;
    }

    public ServiceProvider Services { get; }

    public ManualTimeProvider Time { get; } = new(DateTimeOffset.UtcNow);

    public RecordingMailService Mail { get; } = new();

    public TestCurrentUser CurrentUser { get; } = new();

    public static OrganizationsTestContext Create(string connectionString)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = connectionString,
                ["Auth:Jwt:SigningKey"] = "test-signing-key-with-at-least-32-bytes!",
                ["Mail:Host"] = "localhost",
                ["Mail:FromAddress"] = "noreply@merkwerk.local",
                ["App:PublicBaseUrl"] = "http://localhost:65350",
            })
            .Build();

        var services = new ServiceCollection();
        OrganizationsTestContext? context = null;

        services.AddLogging();
        services.AddSingleton<TimeProvider>(_ => context!.Time);
        services.AddScoped<ICurrentUser>(_ => context!.CurrentUser);
        services.AddMerkwerkDataAccess(configuration);
        services.AddMerkwerkNotifications(configuration);
        services.Replace(ServiceDescriptor.Singleton<IMailService>(_ => context!.Mail));
        services.AddMerkwerkIdentity(configuration);
        services.AddMerkwerkAuthentication(configuration);
        services.AddMerkwerkOrganizations();

        context = new OrganizationsTestContext(services.BuildServiceProvider(validateScopes: true));
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

    /// <summary>A new adult account with a unique address (no membership).</summary>
    public async Task<(long UserId, string Email)> CreateAccountAsync(string displayName = "Anna")
    {
        var email = $"{Guid.NewGuid():N}@example.org";
        var result = await RunAsync<IAccountService, AccountResult>(a => a.CreateAccountAsync(
            new NewAccount(email, displayName, Password, EmailConfirmed: true, PrivacyPolicy.CurrentVersion), default));

        Assert.True(result.Succeeded, string.Join(", ", result.Errors));
        return (result.UserId!.Value, email);
    }

    /// <summary>A family with its owner (admin), like after the first-run setup – without using the one-time setup.</summary>
    public async Task<Family> CreateFamilyAsync(string name = "Familie Test")
    {
        var (ownerId, ownerEmail) = await CreateAccountAsync("Owner");

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

        return new Family(organization.Id, ownerId, ownerEmail);
    }

    /// <summary>Adds an existing account to a family with the given role.</summary>
    public async Task<long> AddMemberAsync(long organizationId, long userId, OrganizationRole role)
    {
        await using var scope = Services.CreateAsyncScope();
        await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>().Create();
        var membership = new Membership { OrganizationId = organizationId, UserId = userId, Role = role };
        unitOfWork.Memberships.Add(membership);
        await unitOfWork.SaveChangesAsync();
        return membership.Id;
    }

    /// <summary>Role and organization claims of a fresh session for the user.</summary>
    public Task<AuthSession?> SignInAsync(long userId) =>
        RunAsync<IAuthSessionService, AuthSession?>(s => s.SignInAsync(userId, default));
}
