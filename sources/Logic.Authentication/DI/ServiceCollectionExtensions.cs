using System.Text;
using Logic.Authentication.Accounts;
using Logic.Authentication.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Logic.Authentication.DI;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers token issuing, refresh tokens in the database, <see cref="IAuthSessionService"/> and
    /// <see cref="IAccountService"/>. Needs Identity's <c>UserManager&lt;User&gt;</c> (registered by Web.Core),
    /// Data.Accessor and Logic.Notifications.
    /// </summary>
    public static IServiceCollection AddMerkwerkAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => Encoding.UTF8.GetByteCount(o.SigningKey) >= 32,
                "Auth:Jwt:SigningKey must be at least 32 bytes. Run deploy/setup-local.ps1 to create it as a user secret.")
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<TokenService>();

        // Scoped: UserManager and the units of work live per request.
        services.AddScoped<RefreshTokenStore>();
        services.AddScoped<AuthSessionService>();
        services.AddScoped<IAuthSessionService>(sp => sp.GetRequiredService<AuthSessionService>());
        services.AddScoped<IAccountService, AccountService>();

        return services;
    }
}
