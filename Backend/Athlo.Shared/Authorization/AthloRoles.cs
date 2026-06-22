namespace Athlo.Shared.Authorization;

public static class AthloRoles
{
    public const string User = nameof(Enums.UserRole.User);
    public const string Admin = nameof(Enums.UserRole.Admin);
    public const string SuperAdmin = nameof(Enums.UserRole.SuperAdmin);
}

public static class AthloPolicies
{
    public const string SuperAdminOnly = "SuperAdminOnly";
    public const string AdminOrSuperAdmin = "AdminOrSuperAdmin";
}
