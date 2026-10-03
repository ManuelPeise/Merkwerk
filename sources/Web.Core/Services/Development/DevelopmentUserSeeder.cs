using Data.Database.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Web.Core.Services.Development;

/// <summary>
/// Creates the development account from "DevelopmentSeed" when the database has no user yet. Does nothing outside
/// Development, without configuration, or before the migrations ran. Replaces the demo user of spike LP-006 until the
/// first-run setup (LP-105) exists.
/// </summary>
public sealed partial class DevelopmentUserSeeder(
    IServiceScopeFactory scopeFactory,
    IHostEnvironment environment,
    IOptions<DevelopmentUserSeederOptions> options,
    ILogger<DevelopmentUserSeeder> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;

        if (!environment.IsDevelopment() || string.IsNullOrEmpty(settings.Email) || string.IsNullOrEmpty(settings.Password))
        {
            return;
        }

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            if (userManager.Users.Any())
            {
                return;
            }

            var user = new User
            {
                UserName = settings.Email,
                Email = settings.Email,
                EmailConfirmed = true,
                DisplayName = settings.DisplayName,
            };
            var result = await userManager.CreateAsync(user, settings.Password);

            if (result.Succeeded)
            {
                LogSeeded();
            }
            else
            {
                LogFailed(string.Join(" ", result.Errors.Select(e => e.Code)));
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Typically: migrations not applied yet. The app still starts.
            LogFailed(exception.GetType().Name);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [LoggerMessage(Level = LogLevel.Information, Message = "Development account created (DevelopmentSeed).")]
    private partial void LogSeeded();

    [LoggerMessage(Level = LogLevel.Warning, Message = "Development account could not be created: {Reason}. Did you run 'dotnet ef database update'?")]
    private partial void LogFailed(string reason);
}
