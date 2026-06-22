using Athlo.Mapper;
using Athlo.Models.DTOs.Admin;
using Athlo.Models.DTOs.Workouts;
using Athlo.Models.Entities;
using Athlo.Repositories;
using Athlo.Repositories.Exercises;
using Athlo.Repositories.Programs;
using Athlo.Repositories.RefreshTokens;
using Athlo.Repositories.Users;
using Athlo.Repositories.Workouts;
using Athlo.Shared.Enums;
using Athlo.Shared.Exceptions;
using Athlo.Shared.Settings;
using Microsoft.Extensions.Options;

namespace Athlo.AuthService.Services;

public class AdminService(
    IUserRepository userRepository,
    IProgramRepository programRepository,
    IExerciseRepository exerciseRepository,
    IWorkoutSessionRepository workoutSessionRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    IOptions<SuperAdminSettings> superAdminOptions) : IAdminService
{
    private readonly string _superAdminEmail = superAdminOptions.Value.Email.Trim().ToLowerInvariant();

    public async Task<IReadOnlyList<AdminUserDto>> GetAdminsAsync(CancellationToken ct = default)
    {
        var users = await userRepository.GetByRolesAsync([UserRole.Admin, UserRole.SuperAdmin], ct);
        return users.Select(AdminMapper.ToDto).ToList();
    }

    public async Task<AdminUserDto> CreateAdminAsync(CreateAdminRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (email == _superAdminEmail)
            throw new ConflictException("This email is reserved for the super admin account.");

        if (await userRepository.EmailExistsAsync(email, ct))
            throw new ConflictException("An account with this email already exists.");

        var admin = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            InitialWeight = 70,
            CurrentWeight = 70,
            GoalWeight = 70,
            FitnessGoal = FitnessGoal.StayActive,
            Role = UserRole.Admin
        };

        await userRepository.AddAsync(admin, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return AdminMapper.ToDto(admin);
    }

    public async Task RemoveAdminAsync(Guid adminId, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(adminId, ct)
            ?? throw new NotFoundException("Admin not found.");

        if (user.Role == UserRole.SuperAdmin)
            throw new AppException("The super admin account cannot be removed.", 403);

        if (user.Role != UserRole.Admin)
            throw new NotFoundException("Admin not found.");

        user.Role = UserRole.User;
        await userRepository.UpdateAsync(user, ct);
        await refreshTokenRepository.RevokeAllForUserAsync(user.Id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<UserListItemDto>> GetUsersAsync(int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (items, totalCount) = await userRepository.GetPagedAsync(page, pageSize, ct);

        return new PagedResult<UserListItemDto>
        {
            Items = items.Select(AdminMapper.ToListItem).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<UserDetailDto> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User not found.");

        return AdminMapper.ToDetail(user);
    }

    public async Task<AdminDashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default)
    {
        var adminCount = await userRepository.CountAsync(UserRole.Admin, ct)
            + await userRepository.CountAsync(UserRole.SuperAdmin, ct);

        return new AdminDashboardStatsDto
        {
            TotalUsers = await userRepository.CountAsync(ct: ct),
            TotalAdmins = adminCount,
            TotalPrograms = await programRepository.CountAsync(ct),
            TotalExercises = await exerciseRepository.CountAsync(ct),
            CompletedWorkoutsToday = await workoutSessionRepository.CountCompletedTodayAsync(ct),
            ActiveWorkoutsNow = await workoutSessionRepository.CountActiveAsync(ct)
        };
    }
}
