using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetGrooming.Migrations
{
    /// <inheritdoc />
    public partial class FixResetTokenColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResetToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetTokenExpiry",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ResetToken", table: "Users");
            migrationBuilder.DropColumn(name: "ResetTokenExpiry", table: "Users");
        }

    }
}
