using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Athlo.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRoleAndActiveSessionIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "User");

            migrationBuilder.CreateIndex(
                name: "IX_workout_sessions_UserId",
                table: "workout_sessions",
                column: "UserId",
                unique: true,
                filter: "\"Status\" = 'InProgress'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_workout_sessions_UserId",
                table: "workout_sessions");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "users");
        }
    }
}
