using System.Net;
using System.Net.Http.Json;
using Athlo.Models.DTOs.Auth;
using Athlo.Shared.Enums;

namespace Athlo.IntegrationTests.Auth;

[Collection("AuthApi")]
public class LoginAttemptApiTests(AuthWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Login_LocksOutAfterRepeatedWrongPasswords()
    {
        var email = $"lockout_{Guid.NewGuid():N}@test.local";
        const string password = "Password123";

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FullName = "Lockout User",
            Email = email,
            Password = password,
            ConfirmPassword = password,
            CurrentWeight = 70,
            GoalWeight = 65,
            FitnessGoal = FitnessGoal.LoseWeight
        });
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var failed = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
            {
                Email = email,
                Password = "WrongPassword1"
            });
            Assert.Equal(HttpStatusCode.Unauthorized, failed.StatusCode);
        }

        var locked = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = password
        });
        Assert.Equal(HttpStatusCode.Unauthorized, locked.StatusCode);
    }

    [Fact]
    public async Task Login_DoesNotLockUnknownEmails()
    {
        var unknownEmail = $"ghost_{Guid.NewGuid():N}@test.local";

        for (var attempt = 0; attempt < 6; attempt++)
        {
            var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
            {
                Email = unknownEmail,
                Password = "WrongPassword1"
            });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
