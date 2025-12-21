using Microsoft.EntityFrameworkCore;
using SmartHome.Core.Entities;
using SmartHomeAPI.Core.Entities;

namespace SmartHome.Infra.Data
{
    public class IndustrialDbContext : DbContext
    {
        public DbSet<SensorTelemetry> SensorTelemetry { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<User> Users { get; set; }

        public DbSet<SecurityAuditRecord> SecurityAudits {get;set;}

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


            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username)
                .HasDatabaseName("ix_users_username");
                entity.HasIndex(e => e.Email).IsUnique()
                .HasDatabaseName("ix_users_email");
                entity.HasIndex(e => e.Role)
                .HasDatabaseName("ix_users_role");
                entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("ix_users_createdat");
                // Настройка свойств
                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50);
                    
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                entity.Property(e => e.HashPassword)
                    .IsRequired()
                    .HasMaxLength(255);
                    
                entity.Property(e => e.Role)
                    .HasConversion<int>(); 
                    
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAdd();
                    
                entity.Property(e => e.UpdatedAt)
                    .ValueGeneratedOnUpdate();
            });

            modelBuilder.Entity<SecurityAuditRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Индексы
                entity.HasIndex(e => e.Timestamp)
                .HasDatabaseName("ix_securityaudits_timestamp");
                entity.HasIndex(e => e.UserId)
                .HasDatabaseName("ix_securityaudits_userid");
                entity.HasIndex(e => e.Action)
                .HasDatabaseName("ix_securityaudits_action");
                entity.HasIndex(e => e.IsSuccessful)
                .HasDatabaseName("ix_securityaudits_issuccessful");
                // Настройка свойств
                entity.Property(e => e.Timestamp)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAdd();
                entity.Property(e => e.IpAddress)
                    .HasMaxLength(45);
                    // Внешний ключ на User
                entity.HasOne(e => e.User)
                    .WithMany(u => u.SecurityAudits)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
                 // При удалении пользователя оставляем запись аудита
            });

        }

    }

}   


