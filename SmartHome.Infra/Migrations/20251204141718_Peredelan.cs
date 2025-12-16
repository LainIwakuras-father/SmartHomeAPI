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

            // 3. Добавляем новый составной PK (Id + Time)
            migrationBuilder.AddPrimaryKey(
                name: "PK_SensorTelemetry",
                table: "SensorTelemetry",
                columns: new[] { "Id", "Time" });

            // Создаем индексы
            migrationBuilder.CreateIndex(
                name: "ix_sensortelemetry_time",
                table: "SensorTelemetry",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "ix_sensortelemetry_sensorid_time",
                table: "SensorTelemetry",
                columns: new[] { "SensorId", "Time" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            // 1. Удаляем индексы (добавлено)
            migrationBuilder.DropIndex(
                name: "ix_sensortelemetry_sensorid_time",
                table: "SensorTelemetry");

            migrationBuilder.DropIndex(
                name: "ix_sensortelemetry_time",
                table: "SensorTelemetry");


            migrationBuilder.DropPrimaryKey(
                name: "PK_SensorTelemetry",
                table: "SensorTelemetry");

         
            migrationBuilder.AddPrimaryKey(
                name: "PK_SensorTelemetry",
                table: "SensorTelemetry",
                column: "Time");
        }
    }
}
