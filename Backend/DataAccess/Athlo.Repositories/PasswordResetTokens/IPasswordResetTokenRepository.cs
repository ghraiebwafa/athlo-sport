using Athlo.Models.Entities;

namespace Athlo.Repositories.PasswordResetTokens;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task AddAsync(PasswordResetToken token, CancellationToken ct = default);
    Task MarkUsedAsync(PasswordResetToken token, CancellationToken ct = default);
    Task InvalidateAllForUserAsync(Guid userId, CancellationToken ct = default);
    Task<int> DeleteExpiredAsync(DateTime olderThan, CancellationToken ct = default);
}
