using System.Security.Claims;
using Athlo.Shared.Authorization;
using Athlo.Shared.Enums;

namespace Athlo.Shared.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : throw new UnauthorizedAccessException();

    public static UserRole GetUserRole(this ClaimsPrincipal user) =>
        Enum.TryParse<UserRole>(user.FindFirstValue(ClaimTypes.Role), out var role)
            ? role
            : UserRole.User;

    public static bool IsSuperAdmin(this ClaimsPrincipal user) =>
        user.IsInRole(AthloRoles.SuperAdmin);

    public static bool IsAdminOrAbove(this ClaimsPrincipal user) =>
        user.IsInRole(AthloRoles.Admin) || user.IsInRole(AthloRoles.SuperAdmin);
}
