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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=localhost\SQLEXPRESS;Database=AutoCarePro;Trusted_Connection=True;");
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
                .IsRequired()
                .HasDefaultValue("CarOwner");

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
        }
    }
} 