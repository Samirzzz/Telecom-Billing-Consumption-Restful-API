using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelecomBilling.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subscribers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PlanType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsRoaming = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscribers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Billings",
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

            migrationBuilder.CreateIndex(
                name: "IX_Subscribers_PhoneNumber",
                table: "Subscribers",
                column: "PhoneNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Billings");

            migrationBuilder.DropTable(
                name: "Subscribers");
        }
    }
}
