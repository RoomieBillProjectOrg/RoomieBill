using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roomiebill.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentReminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    RecurrencePattern = table.Column<int>(type: "int", nullable: false),
                    DayOfMonth = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastReminderSent = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentReminders_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentReminders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReminders_GroupId",
                table: "PaymentReminders",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReminders_UserId",
                table: "PaymentReminders",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentReminders");
        }
    }
}
