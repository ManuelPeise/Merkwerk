using Data.Accessor.DI;
using Data.Database.Abstractions;
using Data.Database.Entities.Identity;
using Logic.Authentication.DI;
using Logic.Notifications.DI;
using Logic.Shared.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

    /// <summary>Creates an adult with a unique e-mail address and returns it.</summary>
    public async Task<UserEntity> CreateUserAsync(string password = "correct-horse-battery", bool emailConfirmed = true)
    {
        var email = $"{Guid.NewGuid():N}@example.org";
        var user = new UserEntity { UserName = email, Email = email, EmailConfirmed = emailConfirmed, DisplayName = "Anna" };

        await RunAsync<UserManager<UserEntity>, bool>(async userManager =>
        {
            var result = await userManager.CreateAsync(user, password);
            Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(e => e.Description)));
            return true;
        });

        return user;
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
