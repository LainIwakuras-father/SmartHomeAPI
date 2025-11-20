using Microsoft.EntityFrameworkCore;
using SmartHome.Core.Entities;

namespace SmartHome.API.Data
{
    public class IndustrialDbContext : DbContext
    {
        public DbSet<SensorTelemetry> SensorTelemetry { get; set; }
        //public DbSet<Sensor> Sensors { get; set; }

        public IndustrialDbContext() { }
        public IndustrialDbContext(DbContextOptions<IndustrialDbContext> options) : base(options) { }

        //создание модели в БД по классу
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SensorTelemetry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Time);
                entity.HasIndex(e => e.SensorId);
                entity.HasIndex(e => new { e.SensorId, e.Time });

                entity.Property(e => e.Time)
                .HasConversion(
                v => v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            });

            //modelBuilder.Entity<Sensor>(entity =>
            //{
            //    entity.HasKey(e => e.Id);
            //    entity.HasIndex(e => e.SensorId).IsUnique();
            //});
        }

    }

}   


