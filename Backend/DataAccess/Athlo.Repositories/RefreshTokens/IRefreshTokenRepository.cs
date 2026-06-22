using Athlo.Models.Entities;

namespace Athlo.Repositories.RefreshTokens;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task AddAsync(RefreshToken token, CancellationToken ct = default);
    Task RevokeAsync(RefreshToken token, CancellationToken ct = default);
    Task<bool> TryRevokeIfActiveAsync(Guid tokenId, CancellationToken ct = default);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);
}
