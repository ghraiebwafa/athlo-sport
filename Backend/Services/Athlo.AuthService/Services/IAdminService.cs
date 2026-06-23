using Athlo.Models.DTOs.Admin;
using Athlo.Models.DTOs.Workouts;

namespace Athlo.AuthService.Services;

public interface IAdminService
{
    Task<IReadOnlyList<AdminUserDto>> GetAdminsAsync(CancellationToken ct = default);
    Task<AdminUserDto> CreateAdminAsync(CreateAdminRequest request, CancellationToken ct = default);
    Task RemoveAdminAsync(Guid adminId, CancellationToken ct = default);
    Task<PagedResult<UserListItemDto>> GetUsersAsync(int page, int pageSize, CancellationToken ct = default);
    Task<UserDetailDto> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
}
