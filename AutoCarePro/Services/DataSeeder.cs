using System;
using System.Collections.Generic;
using AutoCarePro.Models;

namespace AutoCarePro.Services
{
    public class DataSeeder
    {
        private readonly DatabaseService _dbService;

        public DataSeeder(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public void SeedTestData(int userId)
        {
            // Get the user for the vehicles
            var user = _dbService.GetUserById(userId);
            if (user == null) return;

            // Create test vehicles
            var vehicles = new List<Vehicle>
            {
                new Vehicle
                {
                    UserId = userId,
                    User = user,
                    Make = "Toyota",
                    Model = "Camry",
                    Year = 2020,
                    CurrentMileage = 50000,
                    VIN = "1HGCM82633A123456",
                    LicensePlate = "ABC123",
                    FuelType = "Gasoline",
                    Notes = "Primary family vehicle"
                },
                new Vehicle
                {
                    UserId = userId,
                    User = user,
                    Make = "Honda",
                    Model = "Civic",
                    Year = 2019,
                    CurrentMileage = 35000,
                    VIN = "2HGES16575H123456",
                    LicensePlate = "XYZ789",
                    FuelType = "Gasoline",
                    Notes = "Commuter car"
                },
                new Vehicle
                {
                    UserId = userId,
                    User = user,
                    Make = "Ford",
                    Model = "Mustang",
                    Year = 2021,
                    CurrentMileage = 15000,
                    VIN = "1FA6P8TH2M1234567",
                    LicensePlate = "MUS2021",
                    FuelType = "Gasoline",
                    Notes = "Weekend car"
                }
            };

            // Add vehicles to database
            foreach (var vehicle in vehicles)
            {
                _dbService.AddVehicle(vehicle);
            }

            // Add maintenance records for the Toyota Camry (multiple records)
            var camryId = vehicles[0].Id;
            var camry = _dbService.GetVehicleById(camryId);
            var maintenanceRecords = new List<MaintenanceRecord>
            {
                new MaintenanceRecord
                {
                    VehicleId = camryId,
                    Vehicle = camry,
                    MaintenanceType = "Oil Change",
                    Description = "Regular oil change and filter replacement",
                    ServiceProvider = "AutoCare Center",
                    Cost = 75.00m,
                    MaintenanceDate = DateTime.Now.AddMonths(-6),
                    MileageAtMaintenance = 45000,
                    Notes = "Regular maintenance service completed",
                    IsCompleted = true
                },
                new MaintenanceRecord
                {
                    VehicleId = camryId,
                    Vehicle = camry,
                    MaintenanceType = "Brake Service",
                    Description = "Brake pad replacement and rotor resurfacing",
                    ServiceProvider = "AutoCare Center",
                    Cost = 350.00m,
                    MaintenanceDate = DateTime.Now.AddMonths(-3),
                    MileageAtMaintenance = 48000,
                    Notes = "Front and rear brake pads replaced",
                    IsCompleted = true
                },
                new MaintenanceRecord
                {
                    VehicleId = camryId,
                    Vehicle = camry,
                    MaintenanceType = "Tire Rotation",
                    Description = "Tire rotation and balance",
                    ServiceProvider = "Quick Tire Shop",
                    Cost = 45.00m,
                    MaintenanceDate = DateTime.Now.AddMonths(-1),
                    MileageAtMaintenance = 49500,
                    Notes = "Regular tire rotation service",
                    IsCompleted = true
                }
            };

            // Add maintenance records for the Honda Civic
            var civicId = vehicles[1].Id;
            var civic = _dbService.GetVehicleById(civicId);
            maintenanceRecords.Add(new MaintenanceRecord
            {
                VehicleId = civicId,
                Vehicle = civic,
                MaintenanceType = "Oil Change",
                Description = "Synthetic oil change and filter replacement",
                ServiceProvider = "Honda Service Center",
                Cost = 85.00m,
                MaintenanceDate = DateTime.Now.AddMonths(-2),
                MileageAtMaintenance = 32000,
                Notes = "Used synthetic oil as recommended",
                IsCompleted = true
            });

            // Add maintenance records for the Ford Mustang
            var mustangId = vehicles[2].Id;
            var mustang = _dbService.GetVehicleById(mustangId);
            maintenanceRecords.Add(new MaintenanceRecord
            {
                VehicleId = mustangId,
                Vehicle = mustang,
                MaintenanceType = "Inspection",
                Description = "Annual vehicle inspection",
                ServiceProvider = "Ford Dealership",
                Cost = 120.00m,
                MaintenanceDate = DateTime.Now.AddMonths(-1),
                MileageAtMaintenance = 14000,
                Notes = "All systems checked and verified",
                IsCompleted = true
            });

            // Add all maintenance records to database
            foreach (var record in maintenanceRecords)
            {
                _dbService.AddMaintenanceRecord(record);
            }
        }
    }
} 