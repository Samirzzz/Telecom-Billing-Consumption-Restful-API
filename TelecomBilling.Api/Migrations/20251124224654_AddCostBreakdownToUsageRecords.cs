using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelecomBilling.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCostBreakdownToUsageRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BundleExceededDataMB",
                table: "UsageRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BundleExceededMinutes",
                table: "UsageRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CallCost",
                table: "UsageRecords",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DataCost",
                table: "UsageRecords",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsBundleExceeded",
                table: "UsageRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "SMSCost",
                table: "UsageRecords",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCost",
                table: "UsageRecords",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "BundleLimits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VoiceMinutesLimit = table.Column<int>(type: "int", nullable: false),
                    DataMBLimit = table.Column<int>(type: "int", nullable: false),
                    SMSLimit = table.Column<int>(type: "int", nullable: false),
                    PeakTimeMinutesLimit = table.Column<int>(type: "int", nullable: false),
                    OffPeakTimeMinutesLimit = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BundleLimits", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BundleLimits_PlanType",
                table: "BundleLimits",
                column: "PlanType",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BundleLimits");

            migrationBuilder.DropColumn(
                name: "BundleExceededDataMB",
                table: "UsageRecords");

            migrationBuilder.DropColumn(
                name: "BundleExceededMinutes",
                table: "UsageRecords");

            migrationBuilder.DropColumn(
                name: "CallCost",
                table: "UsageRecords");

            migrationBuilder.DropColumn(
                name: "DataCost",
                table: "UsageRecords");

            migrationBuilder.DropColumn(
                name: "IsBundleExceeded",
                table: "UsageRecords");

            migrationBuilder.DropColumn(
                name: "SMSCost",
                table: "UsageRecords");

            migrationBuilder.DropColumn(
                name: "TotalCost",
                table: "UsageRecords");
        }
    }
}
