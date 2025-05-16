using System;
using Microsoft.EntityFrameworkCore;
using AutoCarePro.Data;
using AutoCarePro.Models;

namespace AutoCarePro.Services
{
    public class DatabaseInitializer
    {
        private readonly AutoCareProContext _context;

        public DatabaseInitializer()
        {
            _context = new AutoCareProContext();
        }

        public void Initialize()
        {
            try
            {
                // Ensure database is created
                _context.Database.EnsureCreated();

                // Check if we need to seed data
                if (!_context.Users.Any())
                {
                    SeedInitialData();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error initializing database: " + ex.Message);
            }
        }

        private void SeedInitialData()
        {
            // Create a test user
            var testUser = new User
            {
                Username = "testuser",
                Password = "testpass", // In production, this should be hashed
                Email = "test@example.com",
                FullName = "Test User",
                PhoneNumber = "1234567890",
                Type = UserType.CarOwner,
                CreatedDate = DateTime.Now
            };

            _context.Users.Add(testUser);
            _context.SaveChanges();

            // Create a test car
            var testCar = new Car
            {
                Make = "Toyota",
                Model = "Camry",
                Year = 2020,
                VIN = "1HGCM82633A123456",
                CurrentMileage = 15000,
                LastMaintenanceDate = DateTime.Now.AddMonths(-3),
                EngineType = "V6",
                TransmissionType = "Automatic",
                FuelType = "Gasoline",
                FuelEfficiency = 28.5,
                LastOilChange = DateTime.Now.AddMonths(-3),
                OilChangeMileage = 12000,
                LastTireRotation = DateTime.Now.AddMonths(-4),
                TireRotationMileage = 11000,
                LastBrakeService = DateTime.Now.AddMonths(-6),
                BrakeServiceMileage = 8000,
                UserId = testUser.Id
            };

            _context.Vehicles.Add(testCar);
            _context.SaveChanges();

            // Create a test maintenance record
            var testMaintenance = new MaintenanceRecord
            {
                VehicleId = testCar.Id,
                MaintenanceDate = DateTime.Now.AddMonths(-3),
                MileageAtMaintenance = 12000,
                MaintenanceType = "Oil Change",
                Description = "Regular oil change and filter replacement",
                Cost = 45.00m,
                ServiceProvider = "Toyota Service Center",
                Notes = "Used synthetic oil",
                IsCompleted = true
            };

            _context.MaintenanceRecords.Add(testMaintenance);
            _context.SaveChanges();
        }
    }
} 