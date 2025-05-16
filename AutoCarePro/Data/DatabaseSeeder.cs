using AutoCarePro.Models;
using AutoCarePro.Services;
using System;
using System.Linq;

namespace AutoCarePro.Data
{
    public static class DatabaseSeeder
    {
        public static void Seed(DatabaseService db)
        {
            // Seed Users
            if (!db.GetUsers().Any())
            {
                db.AddUser(new User
                {
                    Username = "carowner",
                    Password = "hashedpassword", // Use a real hash in production!
                    FullName = "Car Owner",
                    Email = "carowner@example.com",
                    PhoneNumber = "1234567890",
                    Type = "Car Owner"
                });

                db.AddUser(new User
                {
                    Username = "maintenancecenter",
                    Password = "hashedpassword", // Use a real hash in production!
                    FullName = "Maintenance Center",
                    Email = "maintenancecenter@example.com",
                    PhoneNumber = "0987654321",
                    Type = "Maintenance Center"
                });
            }

            // Seed Vehicles
            var user = db.GetUsers().FirstOrDefault();
            if (user != null && !db.GetVehiclesByUserId(user.Id).Any())
            {
                db.AddVehicle(new Vehicle
                {
                    UserId = user.Id,
                    Make = "Toyota",
                    Model = "Corolla",
                    Year = 2018,
                    LicensePlate = "ABC123",
                    VIN = "1HGCM82633A004352",
                    CurrentMileage = 50000,
                    FuelType = "Gasoline",
                    TransmissionType = "Automatic",
                    Color = "White",
                    Notes = "Sample vehicle"
                });
            }

            // Seed MaintenanceRecords
            var vehicle = db.GetVehiclesByUserId(user.Id).FirstOrDefault();
            if (vehicle != null && !db.GetMaintenanceRecords(vehicle.Id).Any())
            {
                db.AddMaintenanceRecord(new MaintenanceRecord
                {
                    VehicleId = vehicle.Id,
                    MaintenanceDate = DateTime.Now.AddMonths(-2),
                    MaintenanceType = "Oil Change",
                    Description = "Changed engine oil and filter",
                    MileageAtMaintenance = 48000,
                    Cost = 50,
                    ServiceProvider = "QuickLube",
                    Notes = "No issues"
                });
            }

            // Seed MaintenanceRecommendations
            if (vehicle != null && !db.GetMaintenanceRecommendations(vehicle.Id).Any())
            {
                db.AddMaintenanceRecommendation(new MaintenanceRecommendation
                {
                    VehicleId = vehicle.Id,
                    Component = "Brakes",
                    Description = "Brake pads should be checked soon.",
                    Priority = "Medium",
                    RecommendedDate = DateTime.Now.AddMonths(1),
                    IsCompleted = false
                });
            }
        }
    }
}