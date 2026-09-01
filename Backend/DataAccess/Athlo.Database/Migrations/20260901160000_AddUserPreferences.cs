using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Athlo.Database.Migrations
{
    [DbContext(typeof(Athlo.Database.DbContexts.AthloDbContext))]
    [Migration("20260901160000_AddUserPreferences")]
    public partial class AddUserPreferences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferencesJson",
                table: "users",
                type: "jsonb",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferencesJson",
                table: "users");
        }
    }
}
