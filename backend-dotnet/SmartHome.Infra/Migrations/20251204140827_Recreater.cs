using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHome.Infra.Migrations
{
    /// <inheritdoc />
    public partial class Recreater : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SensorTelemetry",
                table: "SensorTelemetry");

            migrationBuilder.RenameIndex(
                name: "IX_SensorTelemetry_Time",
                table: "SensorTelemetry",
                newName: "ix_sensortelemetry_time");

            migrationBuilder.RenameIndex(
                name: "IX_SensorTelemetry_SensorId_Time",
                table: "SensorTelemetry",
                newName: "ix_sensortelemetry_sensorid_time");

            migrationBuilder.RenameIndex(
                name: "IX_SensorTelemetry_SensorId",
                table: "SensorTelemetry",
                newName: "ix_sensortelemetry_sensorid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SensorTelemetry",
                table: "SensorTelemetry",
                column: "Time");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SensorTelemetry",
                table: "SensorTelemetry");

            migrationBuilder.RenameIndex(
                name: "ix_sensortelemetry_time",
                table: "SensorTelemetry",
                newName: "IX_SensorTelemetry_Time");

            migrationBuilder.RenameIndex(
                name: "ix_sensortelemetry_sensorid_time",
                table: "SensorTelemetry",
                newName: "IX_SensorTelemetry_SensorId_Time");

            migrationBuilder.RenameIndex(
                name: "ix_sensortelemetry_sensorid",
                table: "SensorTelemetry",
                newName: "IX_SensorTelemetry_SensorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SensorTelemetry",
                table: "SensorTelemetry",
                columns: new[] { "Id", "Time" });
        }
    }
}
