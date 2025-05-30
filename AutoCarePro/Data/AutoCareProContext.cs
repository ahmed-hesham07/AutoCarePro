using Microsoft.EntityFrameworkCore;
using AutoCarePro.Models;

namespace AutoCarePro.Data
{
    public class AutoCareProContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        public DbSet<MaintenanceRecommendation> MaintenanceRecommendations { get; set; }
        public DbSet<DiagnosisRecommendation> DiagnosisRecommendations { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Service> Services { get; set; }

        // Parameterless constructor for design-time and runtime use
        public AutoCareProContext() : base()
        {
        }

        public AutoCareProContext(DbContextOptions<AutoCareProContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Use SQLite instead of SQL Server
                optionsBuilder.UseSqlite("Data Source=AutoCarePro.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User Configuration
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Type)
                .HasConversion<string>()
                .IsRequired()
                .HasDefaultValue(UserType.CarOwner);

            // Vehicle Configuration
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.User)
                .WithMany(u => u.Vehicles)
                .HasForeignKey(v => v.UserId);

            // Maintenance Record Configuration
            modelBuilder.Entity<MaintenanceRecord>()
                .HasOne(m => m.Vehicle)
                .WithMany(v => v.MaintenanceHistory)
                .HasForeignKey(m => m.VehicleId);

            // Maintenance Recommendation Configuration
            modelBuilder.Entity<MaintenanceRecommendation>()
                .HasOne(r => r.Vehicle)
                .WithMany(v => v.Recommendations)
                .HasForeignKey(r => r.VehicleId);

            // Diagnosis Recommendation Configuration
            modelBuilder.Entity<DiagnosisRecommendation>()
                .HasOne(d => d.MaintenanceRecord)
                .WithMany(m => m.DiagnosisRecommendations)
                .HasForeignKey(d => d.MaintenanceRecordId);

            // Review Configuration
            modelBuilder.Entity<Review>()
                .HasOne(r => r.ServiceProvider)
                .WithMany()
                .HasForeignKey(r => r.ServiceProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Service Configuration
            modelBuilder.Entity<Service>()
                .HasOne(s => s.ServiceProvider)
                .WithMany()
                .HasForeignKey(s => s.ServiceProviderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}