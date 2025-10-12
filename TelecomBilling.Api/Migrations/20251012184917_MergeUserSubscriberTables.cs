using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelecomBilling.Api.Migrations
{
    /// <inheritdoc />
    public partial class MergeUserSubscriberTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Subscribers_SubscriberId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_UsageRecords_Subscribers_SubscriberId",
                table: "UsageRecords");

            migrationBuilder.DropTable(
                name: "Subscribers");

            migrationBuilder.RenameColumn(
                name: "SubscriberId",
                table: "UsageRecords",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UsageRecords_SubscriberId_Timestamp",
                table: "UsageRecords",
                newName: "IX_UsageRecords_UserId_Timestamp");

            migrationBuilder.RenameColumn(
                name: "SubscriberId",
                table: "Invoices",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Invoices_SubscriberId_Month",
                table: "Invoices",
                newName: "IX_Invoices_UserId_Month");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsRoaming",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PlanType",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "RefreshTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Users_UserId",
                table: "Invoices",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsageRecords_Users_UserId",
                table: "UsageRecords",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Users_UserId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_UsageRecords_Users_UserId",
                table: "UsageRecords");

            migrationBuilder.DropIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsRoaming",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PlanType",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UsageRecords",
                newName: "SubscriberId");

            migrationBuilder.RenameIndex(
                name: "IX_UsageRecords_UserId_Timestamp",
                table: "UsageRecords",
                newName: "IX_UsageRecords_SubscriberId_Timestamp");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Invoices",
                newName: "SubscriberId");

            migrationBuilder.RenameIndex(
                name: "IX_Invoices_UserId_Month",
                table: "Invoices",
                newName: "IX_Invoices_SubscriberId_Month");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "RefreshTokens",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateTable(
                name: "Subscribers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRoaming = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PlanType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscribers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subscribers_PhoneNumber",
                table: "Subscribers",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Subscribers_SubscriberId",
                table: "Invoices",
                column: "SubscriberId",
                principalTable: "Subscribers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsageRecords_Subscribers_SubscriberId",
                table: "UsageRecords",
                column: "SubscriberId",
                principalTable: "Subscribers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
