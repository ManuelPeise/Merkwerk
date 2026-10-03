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
        bool mustChangePassword = false)
    {
        var settings = options.Value;
        var now = timeProvider.GetUtcNow();
        var expiresAt = now.AddMinutes(settings.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(AuthClaims.Subject, userId),
            new(AuthClaims.Name, name),
            new(AuthClaims.Role, role),
        };

        if (mustChangePassword)
        {
            claims.Add(new Claim(AuthClaims.MustChangePassword, "true"));
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

    public static string CreateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    public static SymmetricSecurityKey CreateSigningKey(JwtOptions settings) =>
        new(Encoding.UTF8.GetBytes(settings.SigningKey));
}
