using Athlo.Database.DbContexts;
using Athlo.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Athlo.Repositories.Notifications;

public interface IDevicePushTokenRepository
{
    Task UpsertAsync(Guid userId, string token, string platform, CancellationToken ct = default);
    Task RemoveAsync(Guid userId, string token, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetTokensForUserAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<(Guid UserId, string Token)>> GetAllActiveAsync(CancellationToken ct = default);
}

public class DevicePushTokenRepository(AthloDbContext context) : IDevicePushTokenRepository
{
    public async Task UpsertAsync(Guid userId, string token, string platform, CancellationToken ct = default)
    {
        var existing = await context.DevicePushTokens.FirstOrDefaultAsync(t => t.Token == token, ct);
        if (existing is not null)
        {
            existing.UserId = userId;
            existing.Platform = platform;
            existing.UpdatedAt = DateTime.UtcNow;
            return;
        }

        await context.DevicePushTokens.AddAsync(new DevicePushToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            Platform = platform,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }, ct);
    }

    public async Task RemoveAsync(Guid userId, string token, CancellationToken ct = default)
    {
        var existing = await context.DevicePushTokens
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == token, ct);
        if (existing is not null)
            context.DevicePushTokens.Remove(existing);
    }

    public async Task<IReadOnlyList<string>> GetTokensForUserAsync(Guid userId, CancellationToken ct = default) =>
        await context.DevicePushTokens
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .Select(t => t.Token)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<(Guid UserId, string Token)>> GetAllActiveAsync(CancellationToken ct = default) =>
        await context.DevicePushTokens
            .AsNoTracking()
            .Select(t => new ValueTuple<Guid, string>(t.UserId, t.Token))
            .ToListAsync(ct);
}
