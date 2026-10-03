using System.Linq.Expressions;
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
    /// <summary>Token of a new login, starting the given chain.</summary>
    public async Task<IssuedRefreshToken> IssueAsync(long userId, Guid chainId, CancellationToken cancellationToken)
    {
        await using var unitOfWork = unitOfWorkFactory.Create();
        var issued = Add(unitOfWork, userId, chainId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return issued;
    }

    /// <summary>
    /// Revokes the token and issues its successor in the same chain, in one save. Returns <c>null</c> if the token is
    /// unknown, expired or revoked. A revoked token presented again means it was copied and revokes the whole chain –
    /// except within <see cref="JwtOptions.RefreshTokenReuseSeconds"/> after a rotation whose chain is still alive:
    /// that is a parallel refresh (several tabs, retried request), not theft, and gets a successor as well.
    /// </summary>
    public async Task<IssuedRefreshToken?> RotateAsync(string token, CancellationToken cancellationToken)
    {
        var hash = TokenEncoding.Hash(token);

        try
        {
            return await TryRotateAsync(hash, cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // A parallel request rotated the same token between our read and our save (RevokedAt is a concurrency
            // token). Nothing of ours was written; the second attempt sees the token as just rotated.
            return await TryRotateAsync(hash, cancellationToken);
        }
    }

    /// <summary>Ends the session of the token: revokes its whole chain (logout).</summary>
    public async Task RevokeChainAsync(string token, CancellationToken cancellationToken)
    {
        var hash = TokenEncoding.Hash(token);

        await using var unitOfWork = unitOfWorkFactory.Create();
        var tokens = unitOfWork.Repository<RefreshToken>();
        var chainId = await tokens.Query()
            .Where(t => t.TokenHash == hash)
            .Select(t => (Guid?)t.ChainId)
            .SingleOrDefaultAsync(cancellationToken);

        if (chainId is { } chain)
        {
            await RevokeWhereAsync(tokens, t => t.ChainId == chain, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>Revokes every token of the chain (e.g. the user was locked out meanwhile).</summary>
    public async Task RevokeChainAsync(Guid chainId, CancellationToken cancellationToken)
    {
        await using var unitOfWork = unitOfWorkFactory.Create();
        await RevokeWhereAsync(
            unitOfWork.Repository<RefreshToken>(), t => t.ChainId == chainId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
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

    private async Task<IssuedRefreshToken?> TryRotateAsync(string hash, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        await using var unitOfWork = unitOfWorkFactory.Create();
        var tokens = unitOfWork.Repository<RefreshToken>();
        var stored = await tokens.QueryTracked().SingleOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (stored is null || stored.ExpiresAt <= now)
        {
            return null;
        }

        if (stored.RevokedAt is { } revokedAt)
        {
            if (!await IsParallelRefreshAsync(tokens, stored.ChainId, revokedAt, now, cancellationToken))
            {
                await RevokeWhereAsync(tokens, t => t.ChainId == stored.ChainId, now, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                return null;
            }
        }
        else
        {
            stored.RevokedAt = now;
        }

        var issued = Add(unitOfWork, stored.UserId, stored.ChainId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return issued;
    }

    /// <summary>
    /// Rotated only moments ago and the chain still has a usable token. Logout, password changes and theft detection
    /// revoke the whole chain, so their tokens never pass this check.
    /// </summary>
    private async Task<bool> IsParallelRefreshAsync(
        IRepository<RefreshToken> tokens,
        Guid chainId,
        DateTime revokedAt,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (now - revokedAt > TimeSpan.FromSeconds(options.Value.RefreshTokenReuseSeconds))
        {
            return false;
        }

        return await tokens.Query().AnyAsync(
            t => t.ChainId == chainId && t.RevokedAt == null && t.ExpiresAt > now,
            cancellationToken);
    }

    private IssuedRefreshToken Add(IUnitOfWork unitOfWork, long userId, Guid chainId)
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

        return new IssuedRefreshToken(userId, chainId, token, expiresAt);
    }

    private static async Task RevokeWhereAsync(
        IRepository<RefreshToken> tokens,
        Expression<Func<RefreshToken, bool>> predicate,
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
