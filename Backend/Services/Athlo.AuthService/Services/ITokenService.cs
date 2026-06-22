using Athlo.Models.Entities;
using System.Security.Claims;

namespace Athlo.AuthService.Services;

public interface ITokenService
{
    (string AccessToken, DateTime ExpiresAt) GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Guid? GetUserIdFromClaims(ClaimsPrincipal user);
}
