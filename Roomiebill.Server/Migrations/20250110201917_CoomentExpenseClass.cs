using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roomiebill.Server.Migrations
{
    /// <inheritdoc />
    public partial class CoomentExpenseClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Expenses_ExpenseId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Groups_GroupId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Users_ExpenseId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ExpenseId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Groups",
                newName: "GroupName");

            migrationBuilder.AddColumn<bool>(
                name: "IsLoggedIn",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemAdmin",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Groups_GroupId",
                table: "Users",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Groups_GroupId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsLoggedIn",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsSystemAdmin",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "GroupName",
                table: "Groups",
                newName: "Name");

            migrationBuilder.AddColumn<int>(
                name: "ExpenseId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayerId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: true),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expenses_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Expenses_Users_PayerId",
                        column: x => x.PayerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_ExpenseId",
                table: "Users",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_GroupId",
                table: "Expenses",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_PayerId",
                table: "Expenses",
                column: "PayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Expenses_ExpenseId",
                table: "Users",
                column: "ExpenseId",
                principalTable: "Expenses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Groups_GroupId",
                table: "Users",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");
        }
    }
}
