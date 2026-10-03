using Data.Accessor.DI;
using Data.Database.Abstractions;
using Logic.Authentication;
using Logic.Authentication.DI;
using Logic.Notifications.DI;
using Logic.Organizations.DI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Logic.Shared.DI;
using Web.Core.Services.Authorization;
using Web.Core.Services.Cookies;
using Web.Core.Services.CurrentUser;
using Web.Core.Services.Routing;

namespace Web.Core.Bundels;

/// <summary>Composition root: every service registration of the backend, called once from Program.cs.</summary>
public static class ServiceRegistrationExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Errors as RFC 9457 ProblemDetails (AGENTS.md §8).
        services.AddProblemDetails();

        // Lowercase, kebab-case routes: /api/v1/authentication/forgot-password (LP-104).
        // The refresh cookie path depends on it (AuthCookies).
        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddControllers(options =>
            options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer())));

        // OpenAPI document at /openapi/v1.json; Swagger UI renders it (see AppConfigurationExtensions).
        services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info.Title = "Merkwerk API";
            document.Info.Version = "v1";
            document.Info.Description = "REST API for the Merkwerk web app and the later Expo app.";
            return Task.CompletedTask;
        }));

        services.AddCors(options =>
        {
            var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>();

            if (allowedOrigins == null || allowedOrigins.Length == 0)
            {
                throw new InvalidOperationException("No allowed origins configured for CORS. Please set 'Cors:AllowedOrigins' in appsettings.json.");
            }

            options.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        // Database (LP-101/102): DbContext factory, audit interceptor and unit of work factory;
        // ICurrentUser comes from the access token.
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddMerkwerkDataAccess(configuration);

        // Adult accounts (LP-104): Identity's UserManager, refresh tokens in the database, account mails.
        services.AddMerkwerkIdentity(configuration);
        services.AddMerkwerkAuthentication(configuration);

        // Families, invitations, members, child profiles (LP-105).
        services.AddMerkwerkOrganizations();

        // Mail via SMTP (LP-162): Mailpit in development, MAIL_* from deploy/.env in production.
        services.AddMerkwerkNotifications(configuration);

        services.AddMerkwerkLogicSharedServices(configuration);

        services.AddJwtCookieAuthentication();
        services.AddSingleton<AuthCookieWriter>();
        services.AddAuthorization(AuthorizationPolicies.Configure);


        return services;
    }

    /// <summary>
    /// JWT bearer validation. Browsers send the token in the HttpOnly cookie, native clients in the
    /// Authorization header (ADR 013, confirmed in spike LP-006).
    /// </summary>
    private static void AddJwtCookieAuthentication(this IServiceCollection services)
    {
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
                    NameClaimType = AuthClaims.Name,
                    RoleClaimType = AuthClaims.Role,
                };
                bearer.Events = new JwtBearerEvents
                {
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
    }
}
