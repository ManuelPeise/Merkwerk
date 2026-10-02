using Logic.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Service.Auth;

namespace Service;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers API controllers and JWT bearer validation (token from cookie or Authorization header).</summary>
    public static IServiceCollection AddMerkwerkApi(this IServiceCollection services, IConfiguration configuration)
    {
        // Token issuing and session logic live in Logic.Authentication; this project only does transport.
        services.AddMerkwerkAuthentication(configuration);

        services.AddControllers().AddApplicationPart(typeof(ServiceCollectionExtensions).Assembly);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwt) =>
            {
                var settings = jwt.Value;
                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = TokenService.CreateSigningKey(settings),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(5),
                    NameClaimType = "name",
                    RoleClaimType = "role",
                };
                bearer.Events = new JwtBearerEvents
                {
                    // Browsers send the token in the HttpOnly cookie, native clients in the Authorization header.
                    OnMessageReceived = context =>
                    {
                        if (!context.Request.Headers.ContainsKey("Authorization")
                            && context.Request.Cookies.TryGetValue(AuthCookies.AccessToken, out var token))
                        {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    },
                };
            });

        services.AddAuthorization();

        return services;
    }

    public static IEndpointRouteBuilder MapMerkwerkApi(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers();
        return endpoints;
    }
}
