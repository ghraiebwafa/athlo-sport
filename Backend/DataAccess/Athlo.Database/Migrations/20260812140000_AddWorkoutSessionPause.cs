using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Athlo.Database.Migrations
{
    [DbContext(typeof(Athlo.Database.DbContexts.AthloDbContext))]
    [Migration("20260812140000_AddWorkoutSessionPause")]
    public partial class AddWorkoutSessionPause : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PausedAt",
                table: "workout_sessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PausedDurationSeconds",
                table: "workout_sessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PausedAt",
                table: "workout_sessions");

            migrationBuilder.DropColumn(
                name: "PausedDurationSeconds",
                table: "workout_sessions");
        }
    }
}
