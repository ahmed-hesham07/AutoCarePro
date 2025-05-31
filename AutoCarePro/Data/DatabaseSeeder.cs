using System;
using System.Linq;
using System.Threading.Tasks;
using AutoCarePro.Models;
using AutoCarePro.Services;

namespace AutoCarePro.Data
{
    public static class DatabaseSeeder
    {
        public static async Task Seed(DatabaseService db)
        {
            // Seed users
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "password123",
                UserType = UserType.CarOwner
            };
            await db.AddUserAsync(user);

            // Seed vehicles
            var vehicle = new Vehicle
            {
                Make = "Toyota",
                Model = "Camry",
                Year = 2020,
                VIN = "1HGCM82633A123456",
                Mileage = 50000,
                UserId = user.Id
            };
            await db.AddVehicleAsync(vehicle);

            // Seed maintenance records
            var maintenanceRecord = new MaintenanceRecord
            {
                VehicleId = vehicle.Id,
                ServiceProviderId = user.Id,
                ServiceDate = DateTime.Now,
                MaintenanceType = "Oil Change",
                Description = "Regular oil change service",
                Mileage = 50000,
                Cost = 50.00m,
                Notes = "All good"
            };
            await db.AddMaintenanceRecordAsync(maintenanceRecord);

            // Seed maintenance recommendations
            var recommendation = new MaintenanceRecommendation
            {
                VehicleId = vehicle.Id,
                ServiceProviderId = user.Id,
                RecommendationDate = DateTime.Now,
                Description = "Schedule next oil change in 5000 miles",
                Priority = PriorityLevel.Medium,
                Status = RecommendationStatus.Pending
            };
            await db.AddMaintenanceRecommendationAsync(recommendation);
        }
    }
}