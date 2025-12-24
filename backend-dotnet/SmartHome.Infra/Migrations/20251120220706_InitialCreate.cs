using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartHome.Infra.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SensorTelemetry",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SensorId = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorTelemetry", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SensorTelemetry_SensorId",
                table: "SensorTelemetry",
                column: "SensorId");

            migrationBuilder.CreateIndex(
                name: "IX_SensorTelemetry_SensorId_Time",
                table: "SensorTelemetry",
                columns: new[] { "SensorId", "Time" });

            migrationBuilder.CreateIndex(
                name: "IX_SensorTelemetry_Time",
                table: "SensorTelemetry",
                column: "Time");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SensorTelemetry");
        }
    }
}
