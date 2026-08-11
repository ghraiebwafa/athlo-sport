namespace Athlo.IntegrationTests;

public static class TestConfiguration
{
    public static Dictionary<string, string?> Values { get; } = new()
    {
        ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test",
        ["Jwt:Secret"] = "integration_test_jwt_secret_key_32chars",
        ["Jwt:Issuer"] = "Athlo",
        ["Jwt:Audience"] = "AthloMobile",
        ["Jwt:AccessTokenExpirationMinutes"] = "60",
        ["Jwt:RefreshTokenExpirationDays"] = "7",
        ["SuperAdmin:Email"] = "superadmin@test.local",
        ["SuperAdmin:Password"] = "SuperAdmin123!",
        ["SuperAdmin:FullName"] = "Super Admin",
        ["Cors:AllowedOrigins"] = "http://localhost:8081",
        // Testing only — lets password-reset integration tests assert the token flow.
        ["Auth:ExposeResetTokenInResponse"] = "true"
    };
}
