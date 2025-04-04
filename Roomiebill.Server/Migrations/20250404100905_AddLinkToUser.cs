using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roomiebill.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BitLink",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BitLink",
                table: "Users");
        }
    }
}
