using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelecomBilling.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddConsumptionAndTariffRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Billings");

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriberId = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BillingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VoiceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DataAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SMSAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RoamingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VoiceMinutes = table.Column<int>(type: "int", nullable: false),
                    DataMB = table.Column<int>(type: "int", nullable: false),
                    SMSMessages = table.Column<int>(type: "int", nullable: false),
                    RoamingMinutes = table.Column<int>(type: "int", nullable: false),
                    RoamingDataMB = table.Column<int>(type: "int", nullable: false),
                    RoamingSMSMessages = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Subscribers_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscribers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TariffRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PlanType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VoicePeakRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VoiceOffPeakRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DataRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SMSRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RoamingVoiceRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RoamingDataRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RoamingSMSRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TariffRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsageRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriberId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CallMinutes = table.Column<int>(type: "int", nullable: false),
                    DataMB = table.Column<int>(type: "int", nullable: false),
                    SMSCount = table.Column<int>(type: "int", nullable: false),
                    IsPeakTime = table.Column<bool>(type: "bit", nullable: false),
                    IsRoaming = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsageRecords_Subscribers_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscribers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_SubscriberId_Month",
                table: "Invoices",
                columns: new[] { "SubscriberId", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsageRecords_SubscriberId_Timestamp",
                table: "UsageRecords",
                columns: new[] { "SubscriberId", "Timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "TariffRules");

            migrationBuilder.DropTable(
                name: "UsageRecords");

            migrationBuilder.CreateTable(
                name: "Billings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriberId = table.Column<int>(type: "int", nullable: false),
                    BillingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DataMB = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Month = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoamingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RoamingDataMB = table.Column<int>(type: "int", nullable: false),
                    RoamingMinutes = table.Column<int>(type: "int", nullable: false),
                    RoamingSMSMessages = table.Column<int>(type: "int", nullable: false),
                    SMSAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SMSMessages = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VoiceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VoiceMinutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Billings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Billings_Subscribers_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscribers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Billings_SubscriberId_Month",
                table: "Billings",
                columns: new[] { "SubscriberId", "Month" },
                unique: true);
        }
    }
}
