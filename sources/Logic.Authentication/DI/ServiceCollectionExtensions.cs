using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Logic.Authentication.DI;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers token issuing, refresh-token rotation and <see cref="IAuthSessionService"/>.</summary>
    public static IServiceCollection AddMerkwerkAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => Encoding.UTF8.GetByteCount(o.SigningKey) >= 32,
                "Auth:Jwt:SigningKey must be at least 32 bytes. Run deploy/setup-local.ps1 to create it as a user secret.")
            .ValidateOnStart();

        services.AddOptions<DemoUserOptions>()
            .Bind(configuration.GetSection(DemoUserOptions.SectionName));

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<TokenService>();
        services.AddSingleton<InMemoryRefreshTokenStore>();
        services.AddSingleton<IAuthSessionService, AuthSessionService>();

        return services;
    }
}
