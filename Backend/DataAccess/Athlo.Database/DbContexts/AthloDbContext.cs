using Athlo.Models.Entities;
using Athlo.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Athlo.Database.DbContexts;

public class AthloDbContext : DbContext
{
    public AthloDbContext(DbContextOptions<AthloDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<WorkoutProgram> WorkoutPrograms => Set<WorkoutProgram>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<ProgramExercise> ProgramExercises => Set<ProgramExercise>();
    public DbSet<WorkoutSession> WorkoutSessions => Set<WorkoutSession>();
    public DbSet<WorkoutSetLog> WorkoutSetLogs => Set<WorkoutSetLog>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureRefreshTokens(modelBuilder);
        ConfigurePasswordResetTokens(modelBuilder);
        ConfigureCategories(modelBuilder);
        ConfigureWorkoutPrograms(modelBuilder);
        ConfigureExercises(modelBuilder);
        ConfigureProgramExercises(modelBuilder);
        ConfigureWorkoutSessions(modelBuilder);
        ConfigureWorkoutSetLogs(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(u => u.Id);

            entity.Property(u => u.FullName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(u => u.Email)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(u => u.PasswordHash)
                .IsRequired();

            entity.Property(u => u.InitialWeight).HasPrecision(5, 2);
            entity.Property(u => u.CurrentWeight).HasPrecision(5, 2);
            entity.Property(u => u.GoalWeight).HasPrecision(5, 2);

            entity.Property(u => u.FitnessGoal)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(UserRole.User);

            entity.Property(u => u.CreatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            entity.HasIndex(u => u.Email).IsUnique();
        });
    }

    private static void ConfigureRefreshTokens(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");

            entity.HasKey(t => t.Id);

            entity.Property(t => t.TokenHash)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(t => t.CreatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            entity.HasIndex(t => t.TokenHash).IsUnique();

            entity.HasOne(t => t.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigurePasswordResetTokens(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("password_reset_tokens");

            entity.HasKey(t => t.Id);

            entity.Property(t => t.TokenHash)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(t => t.CreatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            entity.HasIndex(t => t.TokenHash).IsUnique();

            entity.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");

            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(c => c.Slug)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(c => c.Icon)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(c => c.Slug).IsUnique();
        });
    }

    private static void ConfigureWorkoutPrograms(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkoutProgram>(entity =>
        {
            entity.ToTable("workout_programs");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(p => p.Description)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(p => p.ImageUrl)
                .HasMaxLength(500);

            entity.Property(p => p.Difficulty)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.HasOne(p => p.Category)
                .WithMany(c => c.Programs)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureExercises(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.ToTable("exercises");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500);
        });
    }

    private static void ConfigureProgramExercises(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProgramExercise>(entity =>
        {
            entity.ToTable("program_exercises");

            entity.HasKey(pe => pe.Id);
            entity.HasIndex(pe => new { pe.ProgramId, pe.OrderIndex }).IsUnique();

            entity.HasOne(pe => pe.Program)
                .WithMany(p => p.ProgramExercises)
                .HasForeignKey(pe => pe.ProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pe => pe.Exercise)
                .WithMany(e => e.ProgramExercises)
                .HasForeignKey(pe => pe.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureWorkoutSessions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkoutSession>(entity =>
        {
            entity.ToTable("workout_sessions");

            entity.HasKey(s => s.Id);

            entity.Property(s => s.Status)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.HasOne(s => s.User)
                .WithMany(u => u.WorkoutSessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Program)
                .WithMany(p => p.WorkoutSessions)
                .HasForeignKey(s => s.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(s => new { s.UserId, s.Status });
            entity.HasIndex(s => s.CompletedAt);

            entity.HasIndex(s => s.UserId)
                .IsUnique()
                .HasFilter("\"Status\" = 'InProgress'");
        });
    }

    private static void ConfigureWorkoutSetLogs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkoutSetLog>(entity =>
        {
            entity.ToTable("workout_set_logs");

            entity.HasKey(s => s.Id);

            entity.Property(s => s.WeightKg).HasPrecision(8, 2);

            entity.HasOne(s => s.Session)
                .WithMany(ws => ws.SetLogs)
                .HasForeignKey(s => s.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.ProgramExercise)
                .WithMany()
                .HasForeignKey(s => s.ProgramExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Exercise)
                .WithMany()
                .HasForeignKey(s => s.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(s => new { s.SessionId, s.ProgramExerciseId, s.SetNumber }).IsUnique();
            entity.HasIndex(s => new { s.ExerciseId, s.Completed });
        });
    }
}
