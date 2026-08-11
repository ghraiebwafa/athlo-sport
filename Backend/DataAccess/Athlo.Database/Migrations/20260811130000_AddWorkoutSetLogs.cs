using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Athlo.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(Athlo.Database.DbContexts.AthloDbContext))]
    [Migration("20260811130000_AddWorkoutSetLogs")]
    public partial class AddWorkoutSetLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "workout_set_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    SetNumber = table.Column<int>(type: "integer", nullable: false),
                    RepsCompleted = table.Column<int>(type: "integer", nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    Completed = table.Column<bool>(type: "boolean", nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workout_set_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workout_set_logs_exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_workout_set_logs_program_exercises_ProgramExerciseId",
                        column: x => x.ProgramExerciseId,
                        principalTable: "program_exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_workout_set_logs_workout_sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "workout_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_workout_set_logs_ExerciseId_Completed",
                table: "workout_set_logs",
                columns: new[] { "ExerciseId", "Completed" });

            migrationBuilder.CreateIndex(
                name: "IX_workout_set_logs_ProgramExerciseId",
                table: "workout_set_logs",
                column: "ProgramExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_workout_set_logs_SessionId_ProgramExerciseId_SetNumber",
                table: "workout_set_logs",
                columns: new[] { "SessionId", "ProgramExerciseId", "SetNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workout_set_logs");
        }
    }
}
