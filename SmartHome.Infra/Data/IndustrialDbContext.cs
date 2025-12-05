using Microsoft.EntityFrameworkCore;
using SmartHome.Core.Entities;

namespace SmartHome.Infra.Data
{
    public class IndustrialDbContext : DbContext
    {
        public DbSet<SensorTelemetry> SensorTelemetry { get; set; }
        public DbSet<Sensor> Sensors { get; set; }

        public IndustrialDbContext() { }
        public IndustrialDbContext(DbContextOptions<IndustrialDbContext> options) : base(options) { }

        //создание модели в БД по классу
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SensorTelemetry>(entity =>
            {
                
                // 1. СОСТАВНОЙ ПЕРВИЧНЫЙ КЛЮЧ (Id + Time)
                entity.HasKey(e => new { e.Id, e.Time });
                // 2. Автоинкремент только для Id
                entity.Property(e => e.Id)
                      .ValueGeneratedOnAdd(); // EF будет генерировать Id
                entity.HasIndex(e => e.Time)
                 .HasDatabaseName("ix_sensortelemetry_time");
                entity.HasIndex(e => e.SensorId)
                .HasDatabaseName("ix_sensortelemetry_sensorid");
                entity.HasIndex(e => new { e.SensorId, e.Time })
                .HasDatabaseName("ix_sensortelemetry_sensorid_time");

                entity.Property(e => e.Time)
                .HasConversion(
                v => v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            });

            modelBuilder.Entity<Sensor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SensorId).IsUnique();
            });
        }

    }

}   


