using Athlo.Database.DbContexts;
using Athlo.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Athlo.Repositories.RefreshTokens;

public class RefreshTokenRepository(AthloDbContext context) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default) =>
        context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default) =>
        await context.RefreshTokens.AddAsync(token, ct);

    public Task RevokeAsync(RefreshToken token, CancellationToken ct = default)
    {
        token.RevokedAt = DateTime.UtcNow;
        context.RefreshTokens.Update(token);
        return Task.CompletedTask;
    }

    public async Task<bool> TryRevokeIfActiveAsync(Guid tokenId, CancellationToken ct = default)
    {
        var updated = await context.RefreshTokens
            .Where(t => t.Id == tokenId && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, DateTime.UtcNow), ct);

        return updated > 0;
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var tokens = await context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in tokens)
            token.RevokedAt = DateTime.UtcNow;
    }

    public async Task<int> DeleteExpiredAsync(DateTime olderThan, CancellationToken ct = default) =>
        await context.RefreshTokens
            .Where(t => t.ExpiresAt < olderThan || (t.RevokedAt != null && t.RevokedAt < olderThan))
            .ExecuteDeleteAsync(ct);
}
