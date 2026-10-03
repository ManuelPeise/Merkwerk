using Data.Accessor.Abstractions;
using Data.Database.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Logic.Authentication.Tokens;

/// <summary>Refresh tokens in the database, hashed, rotated and revocable (ADR 013, LP-104).</summary>
internal sealed class RefreshTokenStore(
    IUnitOfWorkFactory unitOfWorkFactory,
    IOptions<JwtOptions> options,
    TimeProvider timeProvider)
{
    public async Task<(string Token, DateTimeOffset ExpiresAt)> IssueAsync(
        long userId,
        Guid chainId,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = unitOfWorkFactory.Create();
        var issued = Add(unitOfWork, userId, chainId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return issued;
    }

    /// <summary>
    /// Revokes the token and returns its user and chain, or <c>null</c> if it is unknown, expired or already revoked.
    /// A revoked token being presented again means it was copied: the whole chain is revoked.
    /// </summary>
    public async Task<(long UserId, Guid ChainId)?> RedeemAsync(string token, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var hash = TokenEncoding.Hash(token);

        await using var unitOfWork = unitOfWorkFactory.Create();
        var tokens = unitOfWork.Repository<RefreshToken>();
        var stored = await tokens.QueryTracked().SingleOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (stored is null)
        {
            return null;
        }

        if (stored.RevokedAt is not null)
        {
            await RevokeWhereAsync(tokens, t => t.ChainId == stored.ChainId, now, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return null;
        }

        stored.RevokedAt = now;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return stored.ExpiresAt > now ? (stored.UserId, stored.ChainId) : null;
    }

    public async Task RevokeAsync(string token, CancellationToken cancellationToken)
    {
        var hash = TokenEncoding.Hash(token);

        await using var unitOfWork = unitOfWorkFactory.Create();
        await RevokeWhereAsync(
            unitOfWork.Repository<RefreshToken>(), t => t.TokenHash == hash, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Ends every session of the user (password reset, password change, start password).</summary>
    public async Task RevokeAllAsync(long userId, CancellationToken cancellationToken)
    {
        await using var unitOfWork = unitOfWorkFactory.Create();
        await RevokeWhereAsync(
            unitOfWork.Repository<RefreshToken>(), t => t.UserId == userId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private (string Token, DateTimeOffset ExpiresAt) Add(IUnitOfWork unitOfWork, long userId, Guid chainId)
    {
        var token = TokenService.CreateRefreshToken();
        var expiresAt = timeProvider.GetUtcNow().AddDays(options.Value.RefreshTokenDays);

        unitOfWork.Repository<RefreshToken>().Add(new RefreshToken
        {
            UserId = userId,
            TokenHash = TokenEncoding.Hash(token),
            ChainId = chainId,
            ExpiresAt = expiresAt.UtcDateTime,
        });

        return (token, expiresAt);
    }

    private static async Task RevokeWhereAsync(
        IRepository<RefreshToken> tokens,
        System.Linq.Expressions.Expression<Func<RefreshToken, bool>> predicate,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var active = await tokens.QueryTracked()
            .Where(predicate)
            .Where(t => t.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in active)
        {
            token.RevokedAt = now;
        }
    }
}
