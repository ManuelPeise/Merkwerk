using Data.Database;
using Data.Database.Entities.Identity;
using Logic.Authentication.Accounts;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

namespace Web.Core.Bundels;

/// <summary>
/// ASP.NET Core Identity for adult accounts (LP-104): UserManager with EF stores, password and lockout rules,
/// data-protection tokens for reset and confirmation links. No cookie sign-in – tokens are JWTs (ADR 013).
/// Public so the authentication tests use the same rules.
/// </summary>
public static class IdentityRegistrationExtensions
{
    public const string DataProtectionKeysPathKey = "DataProtection:KeysPath";

    public static IServiceCollection AddMerkwerkIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        // Keys sign the reset/confirmation tokens. In the container they must survive restarts (volume, see docker-compose).
        var dataProtection = services.AddDataProtection().SetApplicationName("Merkwerk");
        var keysPath = configuration[DataProtectionKeysPathKey];
        if (!string.IsNullOrWhiteSpace(keysPath))
        {
            dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keysPath));
        }

        services.AddIdentityCore<User>(options =>
            {
                // Length beats character classes (NIST 800-63B); same rule as the web client (validatePassword).
                options.Password.RequiredLength = 10;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredUniqueChars = 1;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<MerkwerkDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<DataProtectionTokenProviderOptions>(options =>
            options.TokenLifespan = AccountTokenLifetimes.LinkToken);

        return services;
    }
}
