using Athlo.Models.DTOs.Progress;

namespace Athlo.ManagementService.Services;

public interface IProgressService
{
    Task<ProgressResponse> GetProgressAsync(Guid userId, CancellationToken ct = default);
}
