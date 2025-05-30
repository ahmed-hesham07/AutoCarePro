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
                    Type = UserType.CarOwner
                });

                db.AddUser(new User
                {
                    Username = "maintenancecenter",
                    Password = "hashedpassword", // Use a real hash in production!
                    FullName = "Maintenance Center",
                    Email = "maintenancecenter@example.com",
                    PhoneNumber = "0987654321",
                    Type = UserType.MaintenanceCenter
                });
            }

            // Seed Vehicles
            var user = db.GetUsers().FirstOrDefault();
            if (user != null && !db.GetVehiclesByUserId(user.Id).Any())
            {
                var car = new Car
                {
                    UserId = user.Id,
                    Make = "Toyota",
                    Model = "Corolla",
                    Year = 2018,
                    LicensePlate = "ABC123",
                    VIN = "1HGCM82633A004352",
                    CurrentMileage = 50000,
                    FuelType = "Gasoline",
                    Notes = "Sample vehicle",
                    Type = VehicleType.Car,
                    TransmissionType = "Automatic",
                    Color = "White",
                    NumberOfDoors = 4,
                    BodyStyle = "Sedan",
                    EngineType = "4-Cylinder",
                    User = user
                };

                db.AddVehicle(car);
            }

            // Seed Maintenance Records
            var vehicle = db.GetVehiclesByUserId(user.Id).FirstOrDefault();
            if (vehicle != null && !db.GetMaintenanceRecords(vehicle.Id).Any())
            {
                var maintenanceRecord = new MaintenanceRecord
                {
                    VehicleId = vehicle.Id,
                    MaintenanceDate = DateTime.Now,
                    MaintenanceType = "Oil Change",
                    Description = "Regular oil change",
                    MileageAtMaintenance = 50000,
                    Cost = 50.00m,
                    ServiceProvider = "Local Garage",
                    Notes = "Regular maintenance",
                    HasDiagnosisRecommendations = false,
                    Vehicle = vehicle
                };

                db.AddMaintenanceRecord(maintenanceRecord);
            }

            // Seed Maintenance Recommendations
            if (vehicle != null && !db.GetMaintenanceRecommendations(vehicle.Id).Any())
            {
                var recommendation = new MaintenanceRecommendation
                {
                    VehicleId = vehicle.Id,
                    Component = "Brakes",
                    Description = "Brake pads need replacement",
                    Priority = PriorityLevel.High,
                    RecommendedDate = DateTime.Now.AddDays(30),
                    EstimatedCost = 200.00m,
                    Notes = "Regular maintenance",
                    IsCompleted = false,
                    Vehicle = vehicle
                };

                db.AddMaintenanceRecommendation(recommendation);
            }
        }
    }
}