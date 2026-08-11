using System.Net;
using System.Net.Http.Json;
using Athlo.Models.DTOs.Auth;
using Athlo.Shared.Enums;

namespace Athlo.IntegrationTests.Auth;

[Collection("AuthApi")]
public class PasswordResetApiTests(AuthWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ForgotAndResetPassword_Succeeds()
    {
        var email = $"reset_{Guid.NewGuid():N}@test.local";
        const string oldPassword = "Password123";
        const string newPassword = "NewPassword456";

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FullName = "Reset User",
            Email = email,
            Password = oldPassword,
            ConfirmPassword = oldPassword,
            CurrentWeight = 70,
            GoalWeight = 65,
            FitnessGoal = FitnessGoal.LoseWeight
        });
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var forgotResponse = await _client.PostAsJsonAsync("/api/auth/forgot-password", new ForgotPasswordRequest
        {
            Email = email
        });
        Assert.Equal(HttpStatusCode.OK, forgotResponse.StatusCode);

        var forgot = await forgotResponse.Content.ReadFromJsonAsync<ForgotPasswordResponse>(TestJsonOptions.Default);
        Assert.False(string.IsNullOrWhiteSpace(forgot?.ResetToken));

        var resetResponse = await _client.PostAsJsonAsync("/api/auth/reset-password", new ResetPasswordRequest
        {
            Token = forgot!.ResetToken!,
            NewPassword = newPassword,
            ConfirmNewPassword = newPassword
        });
        Assert.Equal(HttpStatusCode.NoContent, resetResponse.StatusCode);

        var oldLogin = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = oldPassword
        });
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);

        var newLogin = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = newPassword
        });
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_UnknownEmail_ReturnsOkWithoutToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", new ForgotPasswordRequest
        {
            Email = $"missing_{Guid.NewGuid():N}@test.local"
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>(TestJsonOptions.Default);
        Assert.NotNull(body);
        Assert.Null(body.ResetToken);
    }
}
