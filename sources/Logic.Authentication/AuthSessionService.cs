using Microsoft.Extensions.Options;

namespace Logic.Authentication;

/// <summary>LP-006 spike implementation: demo user from configuration, refresh tokens in memory.</summary>
internal sealed class AuthSessionService(
    TokenService tokenService,
    InMemoryRefreshTokenStore refreshTokens,
    IOptions<DemoUserOptions> demoUserOptions) : IAuthSessionService
{
    private const string DemoUserId = "demo-1";
    private const string DemoRole = "Member";

    public Task<AuthSession?> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var demo = demoUserOptions.Value;

        if (string.IsNullOrEmpty(demo.DemoUser)
            || !string.Equals(email, demo.DemoUser, StringComparison.OrdinalIgnoreCase)
            || password != demo.DemoPassword)
        {
            return Task.FromResult<AuthSession?>(null);
        }

        return Task.FromResult<AuthSession?>(Issue(DemoUserId, demo.DemoUser, DemoRole));
    }

    public Task<AuthSession?> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        if (!refreshTokens.TryRedeem(refreshToken, out var entry))
        {
            return Task.FromResult<AuthSession?>(null);
        }

        return Task.FromResult<AuthSession?>(Issue(entry.UserId, entry.Name, entry.Role));
    }

    public Task LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        refreshTokens.Revoke(refreshToken);
        return Task.CompletedTask;
    }

    private AuthSession Issue(string userId, string name, string role)
    {
        var (accessToken, accessExpiresAt) = tokenService.CreateAccessToken(userId, name, role);
        var (refreshToken, refreshExpiresAt) = refreshTokens.Issue(userId, name, role);

        return new AuthSession(name, role, accessToken, accessExpiresAt, refreshToken, refreshExpiresAt);
    }
}
