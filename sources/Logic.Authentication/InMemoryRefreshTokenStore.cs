using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Logic.Authentication;

/// <summary>
/// LP-006 spike only: keeps refresh tokens in memory (lost on restart).
/// Replaced by a database table with hashed tokens in LP-104.
/// </summary>
internal sealed class InMemoryRefreshTokenStore(IOptions<JwtOptions> options, TimeProvider timeProvider)
{
    private readonly ConcurrentDictionary<string, RefreshTokenEntry> _entries = new();

    public (string Token, DateTimeOffset ExpiresAt) Issue(string userId, string name, string role)
    {
        var token = TokenService.CreateRefreshToken();
        var expiresAt = timeProvider.GetUtcNow().AddDays(options.Value.RefreshTokenDays);
        _entries[Hash(token)] = new RefreshTokenEntry(userId, name, role, expiresAt);
        return (token, expiresAt);
    }

    /// <summary>Validates and removes the token (rotation: every refresh token can be used once).</summary>
    public bool TryRedeem(string token, out RefreshTokenEntry entry)
    {
        if (_entries.TryRemove(Hash(token), out var found) && found.ExpiresAt > timeProvider.GetUtcNow())
        {
            entry = found;
            return true;
        }

        entry = default!;
        return false;
    }

    public void Revoke(string token) => _entries.TryRemove(Hash(token), out _);

    private static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}

internal sealed record RefreshTokenEntry(string UserId, string Name, string Role, DateTimeOffset ExpiresAt);
