using Data.Accessor.Abstractions;
using Data.Accessor.DI;
using Data.Database.Abstractions;
using Data.Database.Entities.Identity;
using Data.Database.Entities.Organizations;
using Logic.Authentication.DI;
using Logic.Notifications.DI;
using Logic.Shared.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Enums;
using Web.Core.Bundles;

namespace Logic.Authentication.Tests.Infrastructure;

/// <summary>
/// The real registrations (Identity rules from Web.Core, data access, authentication) against the test database,
/// with a manual clock and a mail recorder. Every call runs in its own scope, like one HTTP request.
/// </summary>
public sealed class AuthTestContext
{
    private AuthTestContext(ServiceProvider services, ManualTimeProvider time, RecordingMailService mail)
    {
        Services = services;
        Time = time;
        Mail = mail;
    }

    public ServiceProvider Services { get; }

    public ManualTimeProvider Time { get; }

    public RecordingMailService Mail { get; }

    public static AuthTestContext Create(AuthDatabaseFixture database)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = database.ConnectionString,
                ["Auth:Jwt:SigningKey"] = TestSettings.SigningKey,
                ["Mail:Host"] = "localhost",
                ["Mail:FromAddress"] = "noreply@merkwerk.local",
                ["App:PublicBaseUrl"] = "http://localhost:65350",
            })
            .Build();

        var time = new ManualTimeProvider(DateTimeOffset.UtcNow);
        var mail = new RecordingMailService();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<TimeProvider>(time);
        services.AddScoped<ICurrentUser>(_ => SystemCurrentUser.Instance);
        services.AddMerkwerkDataAccess(configuration);
        services.AddMerkwerkNotifications(configuration);
        services.Replace(ServiceDescriptor.Singleton<IMailService>(mail));
        services.AddMerkwerkIdentity(configuration);
        services.AddMerkwerkAuthentication(configuration);

        return new AuthTestContext(services.BuildServiceProvider(validateScopes: true), time, mail);
    }

    public Task<T> SessionsAsync<T>(Func<IAuthSessionService, Task<T>> action) => RunAsync<IAuthSessionService, T>(action);

    public Task<T> AccountsAsync<T>(Func<IAccountService, Task<T>> action) => RunAsync<IAccountService, T>(action);

    public async Task MigrateAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<Data.Database.MerkwerkDbContext>().Database.MigrateAsync();
    }

    public Task AccountsAsync(Func<IAccountService, Task> action) => RunAsync<IAccountService, bool>(async s =>
    {
        await action(s);
        return true;
    });

    /// <summary>
    /// Creates an adult with a unique e-mail address and returns it. By default the adult gets an own family with the
    /// given role – without a membership there is no session (LP-107).
    /// </summary>
    public async Task<UserEntity> CreateUserAsync(
        string password = "correct-horse-battery",
        bool emailConfirmed = true,
        OrganizationRole? role = OrganizationRole.Member)
    {
        var email = $"{Guid.NewGuid():N}@example.org";
        var user = new UserEntity { UserName = email, Email = email, EmailConfirmed = emailConfirmed, DisplayName = "Anna" };

        await RunAsync<UserManager<UserEntity>, bool>(async userManager =>
        {
            var result = await userManager.CreateAsync(user, password);
            Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(e => e.Description)));
            return true;
        });

        if (role is { } membershipRole)
        {
            await AddToNewFamilyAsync(user.Id, membershipRole);
        }

        return user;
    }

    /// <summary>A new family with the user as its only member.</summary>
    public async Task AddToNewFamilyAsync(long userId, OrganizationRole role)
    {
        await using var scope = Services.CreateAsyncScope();
        await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>().Create();
        var organization = new OrganizationEntity { Name = "Familie Test" };
        unitOfWork.Organizations.Add(organization);
        await unitOfWork.SaveChangesAsync();
        unitOfWork.Memberships.Add(new MembershipEntity { OrganizationId = organization.Id, UserId = userId, Role = role });
        await unitOfWork.SaveChangesAsync();
    }

    /// <summary>Deletes every membership of the user, like a removal from the family.</summary>
    public async Task RemoveMembershipsAsync(long userId)
    {
        await using var scope = Services.CreateAsyncScope();
        await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>().Create();

        while (await unitOfWork.Memberships.FindPrimaryForUserAsync(userId, default) is { } membership)
        {
            unitOfWork.Memberships.Remove(membership);
            await unitOfWork.SaveChangesAsync();
        }
    }

    public Task<UserEntity?> FindUserAsync(long id) =>
        RunAsync<UserManager<UserEntity>, UserEntity?>(userManager => userManager.FindByIdAsync(id.ToString()));

    private async Task<TResult> RunAsync<TService, TResult>(Func<TService, Task<TResult>> action)
        where TService : notnull
    {
        await using var scope = Services.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<TService>());
    }
}
