using Athlo.Database.DbContexts;
using Athlo.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Athlo.Repositories.PasswordResetTokens;

public class PasswordResetTokenRepository(AthloDbContext context) : IPasswordResetTokenRepository
{
    public Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default) =>
        context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task AddAsync(PasswordResetToken token, CancellationToken ct = default) =>
        await context.PasswordResetTokens.AddAsync(token, ct);

    public Task MarkUsedAsync(PasswordResetToken token, CancellationToken ct = default)
    {
        token.UsedAt = DateTime.UtcNow;
        context.PasswordResetTokens.Update(token);
        return Task.CompletedTask;
    }

    public async Task InvalidateAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var tokens = await context.PasswordResetTokens
            .Where(t => t.UserId == userId && t.UsedAt == null)
            .ToListAsync(ct);

        foreach (var token in tokens)
            token.UsedAt = DateTime.UtcNow;
    }
}
