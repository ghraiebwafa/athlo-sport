using Athlo.Models.DTOs.Admin;
using Athlo.Models.Entities;

namespace Athlo.Mapper;

public static class AdminMapper
{
    public static AdminUserDto ToDto(User user) =>
        new()
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };

    public static UserListItemDto ToListItem(User user) =>
        new()
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };

    public static UserDetailDto ToDetail(User user) =>
        new()
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            CurrentWeight = user.CurrentWeight,
            GoalWeight = user.GoalWeight,
            FitnessGoal = user.FitnessGoal,
            CreatedAt = user.CreatedAt
        };
}
