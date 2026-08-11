using Athlo.Mapper;
using Athlo.Models.DTOs.Auth;
using Athlo.Models.Entities;
using Athlo.Repositories;
using Athlo.Repositories.PasswordResetTokens;
using Athlo.Repositories.RefreshTokens;
using Athlo.Repositories.Users;
using Microsoft.AspNetCore.Hosting;
using System.Security.Cryptography;
using Athlo.Shared.Enums;
using Athlo.Shared.Email;
using Athlo.Shared.Exceptions;
using Athlo.Shared.Security;
using Athlo.Shared.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Athlo.AuthService.Services;

public class AuthService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordResetTokenRepository passwordResetTokenRepository,
    IUnitOfWork unitOfWork,
    ITokenService tokenService,
    IOptions<JwtSettings> jwtOptions,
    IOptions<SuperAdminSettings> superAdminOptions,
    IEmailSender emailSender,
    IWebHostEnvironment environment,
    IConfiguration configuration,
    LoginAttemptLimiter loginAttemptLimiter) : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;
    private readonly string _superAdminEmail = superAdminOptions.Value.Email.Trim().ToLowerInvariant();
    private const int PasswordResetExpirationHours = 1;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (email == _superAdminEmail)
            throw new ConflictException("This email is reserved.");

        if (await userRepository.EmailExistsAsync(email, ct))
            throw new ConflictException("An account with this email already exists.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            InitialWeight = request.CurrentWeight,
            CurrentWeight = request.CurrentWeight,
            GoalWeight = request.GoalWeight,
            FitnessGoal = request.FitnessGoal
        };

        await userRepository.AddAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return await CreateAuthResponseAsync(user, ct);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        loginAttemptLimiter.EnsureNotBlocked(email);

        var user = await userRepository.GetByEmailAsync(email, ct);

        if (user is null)
        {
            loginAttemptLimiter.RecordFailure(email);
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            loginAttemptLimiter.RecordFailure(email);
            throw new UnauthorizedException("Invalid email or password.");
        }

        loginAttemptLimiter.Reset(email);
        return await CreateAuthResponseAsync(user, ct);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var tokenHash = TokenHasher.Hash(refreshToken);

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

        var stored = await refreshTokenRepository.GetByTokenHashAsync(tokenHash, ct)
            ?? throw new UnauthorizedException("Invalid refresh token.");

        if (!stored.IsActive)
        {
            if (stored.RevokedAt is not null)
            {
                await refreshTokenRepository.RevokeAllForUserAsync(stored.UserId, ct);
                await unitOfWork.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }

            throw new UnauthorizedException("Refresh token has expired or been revoked.");
        }

        var revoked = await refreshTokenRepository.TryRevokeIfActiveAsync(stored.Id, ct);
        if (!revoked)
        {
            await refreshTokenRepository.RevokeAllForUserAsync(stored.UserId, ct);
            await unitOfWork.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            throw new UnauthorizedException("Invalid refresh token.");
        }

        var response = await CreateAuthResponseAsync(stored.User, ct);
        await transaction.CommitAsync(ct);
        return response;
    }

    public async Task LogoutAsync(Guid userId, string refreshToken, CancellationToken ct = default)
    {
        var tokenHash = TokenHasher.Hash(refreshToken);
        var stored = await refreshTokenRepository.GetByTokenHashAsync(tokenHash, ct);
        if (stored is not null && stored.UserId == userId)
        {
            await refreshTokenRepository.RevokeAsync(stored, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }
    }

    public async Task<UserProfileResponse> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User not found.");

        return UserMapper.ToProfileResponse(user);
    }

    public async Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User not found.");

        if (!string.IsNullOrWhiteSpace(request.FullName))
            user.FullName = request.FullName.Trim();

        if (request.CurrentWeight.HasValue)
            user.CurrentWeight = request.CurrentWeight.Value;

        if (request.GoalWeight.HasValue)
            user.GoalWeight = request.GoalWeight.Value;

        if (request.FitnessGoal.HasValue)
            user.FitnessGoal = request.FitnessGoal.Value;

        await userRepository.UpdateAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return UserMapper.ToProfileResponse(user);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User not found.");

        if (user.Role == UserRole.SuperAdmin)
            throw new AppException("Super admin password must be changed via environment configuration.", 403);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedException("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await userRepository.UpdateAsync(user, ct);
        await refreshTokenRepository.RevokeAllForUserAsync(userId, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        var response = new ForgotPasswordResponse();
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await userRepository.GetByEmailAsync(email, ct);

        if (user is null || user.Role == UserRole.SuperAdmin)
            return response;

        var tokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        await passwordResetTokenRepository.InvalidateAllForUserAsync(user.Id, ct);

        await passwordResetTokenRepository.AddAsync(new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = TokenHasher.Hash(tokenValue),
            ExpiresAt = DateTime.UtcNow.AddHours(PasswordResetExpirationHours)
        }, ct);

        await unitOfWork.SaveChangesAsync(ct);

        await emailSender.SendPasswordResetEmailAsync(user.Email, tokenValue, ct);

        var exposeToken = configuration.GetValue("Auth:ExposeResetTokenInResponse", false);
        var allowExpose = environment.IsDevelopment() || environment.IsEnvironment("Testing");
        if (allowExpose && exposeToken)
            response.ResetToken = tokenValue;

        return response;
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var tokenHash = TokenHasher.Hash(request.Token);
        var stored = await passwordResetTokenRepository.GetByTokenHashAsync(tokenHash, ct);

        if (stored is null || !stored.IsValid)
            throw new UnauthorizedException("Invalid or expired reset token.");

        if (stored.User.Role == UserRole.SuperAdmin)
            throw new AppException("Super admin password must be changed via environment configuration.", 403);

        stored.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await passwordResetTokenRepository.MarkUsedAsync(stored, ct);
        await refreshTokenRepository.RevokeAllForUserAsync(stored.UserId, ct);
        await userRepository.UpdateAsync(stored.User, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(User user, CancellationToken ct)
    {
        var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user);
        var refreshTokenValue = tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = TokenHasher.Hash(refreshTokenValue),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        };

        await refreshTokenRepository.AddAsync(refreshToken, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresAt = expiresAt,
            User = UserMapper.ToProfileResponse(user)
        };
    }
}
