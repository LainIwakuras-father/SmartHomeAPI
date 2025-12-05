using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartHome.Infra.Migrations
{
    /// <inheritdoc />
    public partial class Peredelan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SensorTelemetry",
                table: "SensorTelemetry");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "SensorTelemetry",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SensorTelemetry",
                table: "SensorTelemetry",
                columns: new[] { "Id", "Time" });

            // Создаем индексы
            migrationBuilder.CreateIndex(
                name: "ix_sensortelemetry_time",
                table: "sensor_telemetry",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "ix_sensortelemetry_sensorid_time",
                table: "sensor_telemetry",
                columns: new[] { "SensorId", "Time" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SensorTelemetry",
                table: "SensorTelemetry");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "SensorTelemetry",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SensorTelemetry",
                table: "SensorTelemetry",
                column: "Time");
        }
    }
}
