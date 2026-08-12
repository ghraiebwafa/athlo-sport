using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Athlo.Models.Entities;
using Athlo.Shared.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;

namespace Athlo.IntegrationTests;

public static class TestJwtHelper
{
    public static string CreateAccessToken(User user)
    {
        var settings = TestConfiguration.Values;
        var expiresAt = DateTime.UtcNow.AddMinutes(60);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings["Jwt:Secret"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: settings["Jwt:Issuer"],
            audience: settings["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static User CreateTestUser(string email, UserRole role = UserRole.User)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FullName = $"{role} Integration User",
            Email = email,
            PasswordHash = "hash",
            InitialWeight = 70,
            CurrentWeight = 70,
            GoalWeight = 65,
            FitnessGoal = FitnessGoal.LoseWeight,
            Role = role
        };
    }

    public static HttpClient CreateAuthorizedClient<TEntryPoint>(
        WebApplicationFactory<TEntryPoint> factory,
        UserRole role)
        where TEntryPoint : class
    {
        var client = factory.CreateClient();
        var user = CreateTestUser($"{role.ToString().ToLowerInvariant()}_{Guid.NewGuid():N}@test.local", role);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateAccessToken(user));
        return client;
    }
}
