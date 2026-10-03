using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Logic.Authentication;

/// <summary>Creates signed access tokens and random refresh tokens.</summary>
public sealed class TokenService(IOptions<JwtOptions> options, TimeProvider timeProvider)
{
    private readonly JsonWebTokenHandler _handler = new();

    public (string Token, DateTimeOffset ExpiresAt) CreateAccessToken(
        string userId,
        string name,
        string role,
        bool mustChangePassword = false,
        long? organizationId = null)
    {
        var claims = new List<Claim>
        {
            new(AuthClaims.Subject, userId),
            new(AuthClaims.Name, name),
            new(AuthClaims.Role, role),
        };

        if (organizationId is { } organization)
        {
            claims.Add(new Claim(AuthClaims.OrganizationId, organization.ToString(CultureInfo.InvariantCulture)));
        }

        if (mustChangePassword)
        {
            claims.Add(new Claim(AuthClaims.MustChangePassword, "true"));
        }

        return CreateToken(claims, notAfter: null);
    }

    /// <summary>
    /// Access token of a child on a paired device (LP-106): role <see cref="AuthRoles.Learner"/>, bound to learner, device
    /// and family. Never valid beyond <paramref name="sessionEndsAt"/> (the 8-hour session).
    /// </summary>
    public (string Token, DateTimeOffset ExpiresAt) CreateLearnerAccessToken(
        long learnerId,
        string name,
        string avatarId,
        long organizationId,
        long deviceId,
        DateTimeOffset sessionEndsAt)
    {
        var claims = new List<Claim>
        {
            new(AuthClaims.Subject, learnerId.ToString(CultureInfo.InvariantCulture)),
            new(AuthClaims.Name, name),
            new(AuthClaims.Role, AuthRoles.Learner),
            new(AuthClaims.OrganizationId, organizationId.ToString(CultureInfo.InvariantCulture)),
            new(AuthClaims.LearnerId, learnerId.ToString(CultureInfo.InvariantCulture)),
            new(AuthClaims.DeviceId, deviceId.ToString(CultureInfo.InvariantCulture)),
            new(AuthClaims.AvatarId, avatarId),
        };

        return CreateToken(claims, sessionEndsAt);
    }

    public static string CreateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    public static SymmetricSecurityKey CreateSigningKey(JwtOptions settings) =>
        new(Encoding.UTF8.GetBytes(settings.SigningKey));

    private (string Token, DateTimeOffset ExpiresAt) CreateToken(List<Claim> claims, DateTimeOffset? notAfter)
    {
        var settings = options.Value;
        var now = timeProvider.GetUtcNow();
        var expiresAt = now.AddMinutes(settings.AccessTokenMinutes);

        if (notAfter is { } limit && limit < expiresAt)
        {
            expiresAt = limit;
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(CreateSigningKey(settings), SecurityAlgorithms.HmacSha256),
        };

        return (_handler.CreateToken(descriptor), expiresAt);
    }
}
