using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roomiebill.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddOneToManyInviteUser_20 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invites_Users_UserId",
                table: "Invites");

            migrationBuilder.DropIndex(
                name: "IX_Invites_UserId",
                table: "Invites");

            migrationBuilder.DropColumn(
                name: "InviteeUsername",
                table: "Invites");

            migrationBuilder.DropColumn(
                name: "InviterUsername",
                table: "Invites");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Invites");

            migrationBuilder.AddColumn<int>(
                name: "InvitedId",
                table: "Invites",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InviterId",
                table: "Invites",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Invites_InvitedId",
                table: "Invites",
                column: "InvitedId");

            migrationBuilder.CreateIndex(
                name: "IX_Invites_InviterId",
                table: "Invites",
                column: "InviterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invites_Users_InvitedId",
                table: "Invites",
                column: "InvitedId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invites_Users_InviterId",
                table: "Invites",
                column: "InviterId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invites_Users_InvitedId",
                table: "Invites");

            migrationBuilder.DropForeignKey(
                name: "FK_Invites_Users_InviterId",
                table: "Invites");

            migrationBuilder.DropIndex(
                name: "IX_Invites_InvitedId",
                table: "Invites");

            migrationBuilder.DropIndex(
                name: "IX_Invites_InviterId",
                table: "Invites");

            migrationBuilder.DropColumn(
                name: "InvitedId",
                table: "Invites");

            migrationBuilder.DropColumn(
                name: "InviterId",
                table: "Invites");

            migrationBuilder.AddColumn<string>(
                name: "InviteeUsername",
                table: "Invites",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InviterUsername",
                table: "Invites",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Invites",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invites_UserId",
                table: "Invites",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invites_Users_UserId",
                table: "Invites",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
