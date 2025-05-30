using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoCarePro.Data;
using AutoCarePro.Models;

namespace AutoCarePro.Services
{
    /// <summary>
    /// DatabaseInitializer class is responsible for setting up and initializing the database.
    /// It ensures the database exists and contains initial test data if needed.
    /// </summary>
    public class DatabaseInitializer : IDisposable
    {
        // Database context for Entity Framework operations
        private readonly AutoCareProContext _context;
        private bool _disposed = false;

        /// <summary>
        /// Constructor initializes the database context
        /// </summary>
        public DatabaseInitializer()
        {
            var options = new DbContextOptionsBuilder<AutoCareProContext>()
                .UseSqlite(GetConnectionString())
                .Options;
            _context = new AutoCareProContext(options);
        }

        private string GetConnectionString()
        {
            return "Data Source=AutoCarePro.db";
        }

        /// <summary>
        /// Initializes the database by ensuring it exists and seeding initial data if needed
        /// </summary>
        /// <exception cref="Exception">Thrown when database initialization fails</exception>
        public async Task InitializeAsync()
        {
            try
            {
                // Create the database if it doesn't exist and apply migrations
                await _context.Database.MigrateAsync();

                // Only seed data if the database is empty (no users exist)
                if (!await _context.Users.AnyAsync())
                {
                    await SeedInitialDataAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error initializing database", ex);
            }
        }

        /// <summary>
        /// Seeds the database with initial test data
        /// This includes a test user, a test car, and a test maintenance record
        /// </summary>
        private async Task SeedInitialDataAsync()
        {
            try
            {
                // Create a test user with basic information
                var testPassword = "test123";
                var hashedPassword = HashPassword(testPassword);
                Console.WriteLine($"Creating test user with password hash: {hashedPassword}");

                var testUser = new User
                {
                    Username = "test",
                    Password = hashedPassword,
                    Email = "test@example.com",
                    FullName = "Test User",
                    PhoneNumber = "1234567890",
                    Type = UserType.CarOwner,
                    CreatedDate = DateTime.Now
                };

                _context.Users.Add(testUser);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Test user created with ID: {testUser.Id}");

                // Verify the user was created
                var createdUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "test");
                if (createdUser != null)
                {
                    Console.WriteLine($"Verified test user exists in database with ID: {createdUser.Id}");
                    Console.WriteLine($"Stored password hash: {createdUser.Password}");
                }
                else
                {
                    Console.WriteLine("Failed to verify test user creation!");
                }

                // Create a test car with detailed specifications
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
                    UserId = testUser.Id,
                    Color = "White",
                    BodyStyle = "Sedan",
                    LicensePlate = "ABC123",
                    Notes = "Test vehicle",
                    User = testUser
                };

                _context.Cars.Add(testCar);
                await _context.SaveChangesAsync();

                // Create a test maintenance record for the car
                var testMaintenance = new MaintenanceRecord
                {
                    VehicleId = testCar.Id,
                    MaintenanceDate = DateTime.Now.AddMonths(-3),
                    MaintenanceType = "Oil Change",
                    Description = "Regular oil change and filter replacement",
                    MileageAtMaintenance = 12000,
                    Cost = 45.00m,
                    ServiceProvider = "Toyota Service Center",
                    Notes = "Used synthetic oil",
                    IsCompleted = true,
                    Vehicle = testCar
                };

                _context.MaintenanceRecords.Add(testMaintenance);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error seeding initial data", ex);
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DatabaseInitializer()
        {
            Dispose(false);
        }
    }
} 