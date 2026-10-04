using Data.Accessor.Abstractions;
using Data.Accessor.DI;
using Data.Database;
using Data.Database.Abstractions;
using Data.Database.Entities.Organizations;
using Logic.Authentication.DI;
using Logic.Content.DI;
using Logic.Notifications.DI;
using Logic.Organizations;
using Logic.Organizations.DI;
using Logic.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Enums;
using Shared.Models.Authentication;
using Web.Core.Bundles;

namespace Logic.Content.Tests.Infrastructure;

/// <summary>
/// The real registrations against the test database. Every call runs in its own scope, like one HTTP request.
/// </summary>
public sealed class ContentTestContext
{
    private const string Password = "correct-horse-battery";

    private ContentTestContext(ServiceProvider services)
    {
        Services = services;
    }

    public ServiceProvider Services { get; }

    public TestCurrentUser CurrentUser { get; } = new();

    public static ContentTestContext Create(string connectionString)
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
        ContentTestContext? context = null;

        services.AddLogging();
        services.AddScoped<ICurrentUser>(_ => context!.CurrentUser);
        services.AddMerkwerkDataAccess(configuration);
        services.AddMerkwerkNotifications(configuration);
        services.AddMerkwerkIdentity(configuration);
        services.AddMerkwerkAuthentication(configuration);
        services.AddMerkwerkOrganizations();
        services.AddMerkwerkContent();

        context = new ContentTestContext(services.BuildServiceProvider(validateScopes: true));
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

    /// <summary>A new family with an admin and a member; the "request" works in it from now on.</summary>
    public async Task<Family> CreateFamilyAsync()
    {
        var adminId = await CreateAccountAsync("Admin");
        var memberId = await CreateAccountAsync("Member");

        await using var scope = Services.CreateAsyncScope();
        await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>().Create();
        var organization = new OrganizationEntity { Name = "Familie Test" };
        unitOfWork.Organizations.Add(organization);
        await unitOfWork.SaveChangesAsync();
        unitOfWork.Memberships.Add(new MembershipEntity
        {
            OrganizationId = organization.Id,
            UserId = adminId,
            Role = OrganizationRole.OrgAdmin,
            IsOwner = true,
        });
        unitOfWork.Memberships.Add(new MembershipEntity
        {
            OrganizationId = organization.Id,
            UserId = memberId,
            Role = OrganizationRole.Member,
        });
        await unitOfWork.SaveChangesAsync();

        CurrentUser.OrganizationId = organization.Id;
        return new Family(organization.Id, adminId, memberId);
    }

    private async Task<long> CreateAccountAsync(string displayName)
    {
        var email = $"{Guid.NewGuid():N}@example.org";
        var result = await RunAsync<IAccountService, AccountResult>(a => a.CreateAccountAsync(
            new NewAccount(email, displayName, Password, EmailConfirmed: true, PrivacyPolicy.CurrentVersion), default));

        Assert.True(result.Succeeded, string.Join(", ", result.Errors));
        return result.UserId!.Value;
    }
}
