using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roomiebill.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddFirebaseTokenToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirebaseToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirebaseToken",
                table: "Users");
        }
    }
}
