using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AutoCarePro.Models;
using AutoCarePro.Data;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AutoCarePro.Services
{
    /// <summary>
    /// DatabaseService class handles all database operations for the AutoCarePro application.
    /// This class serves as the central data access layer, managing all interactions with the database.
    /// 
    /// Key features:
    /// 1. User authentication and management
    /// 2. Vehicle registration and tracking
    /// 3. Maintenance record management
    /// 4. Diagnosis and recommendation handling
    /// 5. Password reset functionality
    /// 
    /// Security features:
    /// - Password hashing and verification
    /// - Data validation before operations
    /// - Secure password reset process
    /// - User authentication checks
    /// 
    /// Usage:
    /// This service is used throughout the application to:
    /// - Manage user accounts and authentication
    /// - Track vehicle maintenance
    /// - Handle maintenance recommendations
    /// - Process diagnosis records
    /// </summary>
    public class DatabaseService : IDisposable
    {
        /// <summary>
        /// Database context for Entity Framework operations.
        /// This provides access to the database and manages database connections.
        /// </summary>
        private readonly AutoCareProContext _context;

        /// <summary>
        /// Service for validating data before database operations.
        /// This ensures data integrity and business rule compliance.
        /// </summary>
        private readonly UnifiedValidationService _validationService;

        private readonly ILogger<DatabaseService> _logger;

        private bool _disposed = false;

        /// <summary>
        /// Constructor initializes the database context and validation service.
        /// This sets up the necessary components for database operations.
        /// </summary>
        public DatabaseService(ILogger<DatabaseService> logger)
        {
            var options = new DbContextOptionsBuilder<AutoCareProContext>()
                .UseSqlite(GetConnectionString())
                .Options;
            _context = new AutoCareProContext(options);
            _validationService = new UnifiedValidationService();
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return "Data Source=AutoCarePro.db";
        }

        #region Authentication Methods

        /// <summary>
        /// Authenticates a user with the given username and password.
        /// This method verifies user credentials and returns the user if authentication is successful.
        /// 
        /// Process:
        /// 1. Checks username and password match
        /// 2. Includes related vehicle data
        /// 3. Returns user object if authenticated
        /// 
        /// Security:
        /// - Password should be hashed before calling this method
        /// - Returns null for invalid credentials
        /// </summary>
        /// <param name="username">The username to authenticate</param>
        /// <param name="hashedPassword">The hashed password to verify</param>
        /// <returns>The authenticated user if successful, null otherwise</returns>
        public User AuthenticateUser(string username, string hashedPassword)
        {
            return _context.Users
                .Include(u => u.Vehicles)
                .FirstOrDefault(u => u.Username == username && u.Password == hashedPassword);
        }

        /// <summary>
        /// Registers a new user in the system.
        /// This method handles the complete user registration process.
        /// 
        /// Process:
        /// 1. Validates user data
        /// 2. Checks for duplicate username/email
        /// 3. Adds user to database
        /// 4. Saves changes
        /// 
        /// Validation:
        /// - Username must be unique
        /// - Email must be unique
        /// - All required fields must be valid
        /// </summary>
        /// <param name="user">The user to register</param>
        /// <returns>True if registration was successful, false otherwise</returns>
        /// <exception cref="ValidationException">Thrown when user data is invalid</exception>
        public bool RegisterUser(User user)
        {
            try
            {
                // Validate user data before registration
                var validationResult = _validationService.ValidateUser(user);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.GetErrorMessage());
                }

                // Check for duplicate username
                if (_context.Users.Any(u => u.Username == user.Username))
                {
                    return false;
                }

                // Check for duplicate email
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    return false;
                }

                _context.Users.Add(user);
                _context.SaveChanges();
                return true;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Updates an existing user's information.
        /// This method handles updating user profile data.
        /// 
        /// Process:
        /// 1. Validates updated user data
        /// 2. Finds existing user
        /// 3. Updates user information
        /// 4. Saves changes
        /// 
        /// Validation:
        /// - All updated fields must be valid
        /// - User must exist in database
        /// </summary>
        /// <param name="user">The user with updated information</param>
        /// <exception cref="ValidationException">Thrown when user data is invalid</exception>
        public void UpdateUser(User user)
        {
            var validationResult = _validationService.ValidateUser(user);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.GetErrorMessage());
            }

            var existingUser = _context.Users.Find(user.Id);
            if (existingUser != null)
            {
                _context.Entry(existingUser).CurrentValues.SetValues(user);
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// Changes a user's password.
        /// This method handles the password change process.
        /// 
        /// Process:
        /// 1. Verifies current password
        /// 2. Updates to new password
        /// 3. Saves changes
        /// 
        /// Security:
        /// - Current password must match
        /// - New password should be hashed before calling
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="currentPassword">The current password</param>
        /// <param name="newPassword">The new password</param>
        /// <returns>True if password change was successful, false otherwise</returns>
        public bool ChangePassword(int userId, string currentPassword, string newPassword)
        {
            var user = _context.Users.Find(userId);
            if (user != null && user.Password == currentPassword)
            {
                user.Password = newPassword;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        #endregion

        #region User Operations

        /// <summary>
        /// Retrieves a user by their ID.
        /// This method gets a user's complete profile including their vehicles.
        /// 
        /// Process:
        /// 1. Looks up user by ID
        /// 2. Includes related vehicle data
        /// 3. Returns user object
        /// </summary>
        /// <param name="id">The user ID to look up</param>
        /// <returns>The user if found, null otherwise</returns>
        public User GetUserById(int id)
        {
            return _context.Users
                .Include(u => u.Vehicles)
                .FirstOrDefault(u => u.Id == id);
        }

        /// <summary>
        /// Retrieves a user by their username.
        /// This method gets a user's complete profile including their vehicles.
        /// 
        /// Process:
        /// 1. Looks up user by username
        /// 2. Includes related vehicle data
        /// 3. Returns user object
        /// </summary>
        /// <param name="username">The username to look up</param>
        /// <returns>The user if found, null otherwise</returns>
        public User GetUserByUsername(string username)
        {
            return _context.Users
                .Include(u => u.Vehicles)
                .FirstOrDefault(u => u.Username == username);
        }

        /// <summary>
        /// Gets all users from the database.
        /// </summary>
        /// <returns>A list of all users in the system</returns>
        public List<User> GetUsers()
        {
            return _context.Users.ToList();
        }

        /// <summary>
        /// Adds a new user to the database.
        /// </summary>
        /// <param name="user">The user to add</param>
        /// <returns>True if the user was added successfully, false otherwise</returns>
        public bool AddUser(User user)
        {
            try
            {
                _context.Users.Add(user);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Vehicle Operations

        /// <summary>
        /// Gets all vehicles owned by a specific user.
        /// This method retrieves the complete vehicle list with maintenance history.
        /// 
        /// Process:
        /// 1. Finds all vehicles for user
        /// 2. Includes maintenance history
        /// 3. Includes recommendations
        /// 4. Returns vehicle list
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>List of vehicles owned by the user</returns>
        public List<Vehicle> GetVehiclesByUserId(int userId)
        {
            return _context.Vehicles
                .Include(v => v.MaintenanceHistory)
                .Include(v => v.Recommendations)
                .Where(v => v.UserId == userId)
                .ToList();
        }

        /// <summary>
        /// Gets a specific vehicle by its ID.
        /// This method retrieves a vehicle with its complete maintenance history.
        /// 
        /// Process:
        /// 1. Finds vehicle by ID
        /// 2. Includes maintenance history
        /// 3. Includes recommendations
        /// 4. Returns vehicle object
        /// </summary>
        /// <param name="id">The vehicle ID to look up</param>
        /// <returns>The vehicle if found, null otherwise</returns>
        public async Task<Vehicle?> GetVehicleByIdAsync(int id)
        {
            try
            {
                return await _context.Vehicles
                    .Include(v => v.User)
                    .Include(v => v.MaintenanceRecords)
                    .FirstOrDefaultAsync(v => v.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle by ID: {Id}", id);
                return null;
            }
        }

        /// <summary>
        /// Updates an existing vehicle in the database.
        /// This method handles the complete vehicle update process.
        /// 
        /// Process:
        /// 1. Validates vehicle data
        /// 2. Finds existing vehicle
        /// 3. Updates vehicle information
        /// 4. Saves changes
        /// 
        /// Validation:
        /// - All updated fields must be valid
        /// - Vehicle must exist in database
        /// </summary>
        /// <param name="vehicle">The vehicle with updated information</param>
        /// <exception cref="ValidationException">Thrown when vehicle data is invalid</exception>
        public void UpdateVehicle(Vehicle vehicle)
        {
            var validationResult = _validationService.ValidateVehicle(vehicle);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.GetErrorMessage());
            }

            var existingVehicle = _context.Vehicles.Find(vehicle.Id);
            if (existingVehicle != null)
            {
                _context.Entry(existingVehicle).CurrentValues.SetValues(vehicle);
                _context.SaveChanges();
            }
        }

        #endregion

        #region Maintenance Operations

        /// <summary>
        /// Gets all maintenance records for a specific vehicle.
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle</param>
        /// <returns>A list of maintenance records for the vehicle</returns>
        public List<MaintenanceRecord> GetMaintenanceRecords(int vehicleId)
        {
            return _context.MaintenanceRecords
                .Where(m => m.VehicleId == vehicleId)
                .OrderByDescending(m => m.ServiceDate)
                .ToList();
        }

        /// <summary>
        /// Gets all maintenance recommendations for a specific vehicle.
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle</param>
        /// <returns>A list of maintenance recommendations for the vehicle</returns>
        public List<MaintenanceRecommendation> GetMaintenanceRecommendations(int vehicleId)
        {
            return _context.MaintenanceRecommendations
                .Where(r => r.VehicleId == vehicleId)
                .OrderByDescending(r => r.RecommendedDate)
                .ToList();
        }

        /// <summary>
        /// Gets a specific maintenance record by its ID.
        /// This method retrieves a maintenance record with its associated vehicle and service provider.
        /// 
        /// Process:
        /// 1. Finds record by ID
        /// 2. Includes related vehicle and service provider data
        /// 3. Returns record object
        /// </summary>
        /// <param name="id">The ID of the maintenance record</param>
        /// <returns>The maintenance record if found, null otherwise</returns>
        public async Task<MaintenanceRecord?> GetMaintenanceRecordByIdAsync(int id)
        {
            try
            {
                return await _context.MaintenanceRecords
                    .Include(r => r.Vehicle)
                    .Include(r => r.ServiceProvider)
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting maintenance record by ID: {Id}", id);
                return null;
            }
        }

        /// <summary>
        /// Gets all maintenance records for a specific vehicle.
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle</param>
        /// <returns>A list of maintenance records for the vehicle</returns>
        public async Task<List<MaintenanceRecord>> GetMaintenanceRecordsByVehicle(int vehicleId)
        {
            using var context = new AutoCareProContext();
            return await context.MaintenanceRecords
                .Include(r => r.ServiceProvider)
                .Where(r => r.VehicleId == vehicleId)
                .OrderByDescending(r => r.ServiceDate)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new maintenance record to the database.
        /// This method handles the complete maintenance record creation process.
        /// 
        /// Process:
        /// 1. Validates maintenance data
        /// 2. Adds record to database
        /// 3. Updates vehicle maintenance date
        /// 4. Saves changes
        /// 
        /// Validation:
        /// - All required fields must be valid
        /// - Vehicle must exist
        /// - Maintenance date must be valid
        /// </summary>
        /// <param name="record">The maintenance record to add</param>
        /// <exception cref="ValidationException">Thrown when maintenance data is invalid</exception>
        public async Task<bool> AddMaintenanceRecordAsync(MaintenanceRecord record)
        {
            try
            {
                await _context.MaintenanceRecords.AddAsync(record);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding maintenance record: {Record}", record);
                return false;
            }
        }

        /// <summary>
        /// Deletes a maintenance record from the database.
        /// This method handles the complete maintenance record removal process.
        /// 
        /// Process:
        /// 1. Finds record by ID
        /// 2. Removes associated diagnosis recommendations
        /// 3. Deletes record
        /// 4. Saves changes
        /// 
        /// Note:
        /// This operation is permanent and cannot be undone.
        /// </summary>
        /// <param name="recordId">The ID of the maintenance record to delete</param>
        public async Task<bool> DeleteMaintenanceRecord(int id)
        {
            try
            {
                var record = await _context.MaintenanceRecords.FindAsync(id);
                if (record == null)
                {
                    return false;
                }

                _context.MaintenanceRecords.Remove(record);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting maintenance record");
                return false;
            }
        }

        /// <summary>
        /// Updates an existing maintenance record in the database.
        /// This method handles updating maintenance record data.
        /// 
        /// Process:
        /// 1. Validates updated record data
        /// 2. Finds existing record
        /// 3. Updates record information
        /// 4. Saves changes
        /// 
        /// Validation:
        /// - All updated fields must be valid
        /// - Record must exist in database
        /// </summary>
        /// <param name="record">The maintenance record with updated information</param>
        /// <exception cref="ValidationException">Thrown when record data is invalid</exception>
        public async Task<bool> UpdateMaintenanceRecordAsync(MaintenanceRecord record)
        {
            try
            {
                _context.MaintenanceRecords.Update(record);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating maintenance record: {Record}", record);
                return false;
            }
        }

        /// <summary>
        /// Adds a new maintenance recommendation to the database.
        /// This method handles the complete recommendation creation process.
        /// 
        /// Process:
        /// 1. Validates recommendation data
        /// 2. Adds recommendation to database
        /// 3. Saves changes
        /// 
        /// Validation:
        /// - All required fields must be valid
        /// - Vehicle must exist
        /// - Recommendation date must be valid
        /// </summary>
        /// <param name="recommendation">The maintenance recommendation to add</param>
        /// <exception cref="ValidationException">Thrown when recommendation data is invalid</exception>
        public void AddMaintenanceRecommendation(MaintenanceRecommendation recommendation)
        {
            var validationResult = _validationService.ValidateMaintenanceRecommendation(recommendation);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.GetErrorMessage());
            }

            _context.MaintenanceRecommendations.Add(recommendation);
            _context.SaveChanges();
        }

        /// <summary>
        /// Adds a new diagnosis recommendation to the database.
        /// This method handles the complete diagnosis recommendation creation process.
        /// 
        /// Process:
        /// 1. Validates diagnosis data
        /// 2. Adds recommendation to database
        /// 3. Updates maintenance record
        /// 4. Saves changes
        /// 
        /// Validation:
        /// - All required fields must be valid
        /// - Maintenance record must exist
        /// - Diagnosing user must be a maintenance center
        /// </summary>
        /// <param name="recommendation">The diagnosis recommendation to add</param>
        /// <exception cref="ValidationException">Thrown when diagnosis data is invalid</exception>
        public void AddDiagnosisRecommendation(DiagnosisRecommendation recommendation)
        {
            var validationResult = _validationService.ValidateDiagnosisRecommendation(recommendation);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.GetErrorMessage());
            }

            _context.DiagnosisRecommendations.Add(recommendation);
            
            // Update maintenance record
            var maintenanceRecord = _context.MaintenanceRecords.Find(recommendation.MaintenanceRecordId);
            if (maintenanceRecord != null)
            {
                maintenanceRecord.HasDiagnosisRecommendations = true;
            }

            _context.SaveChanges();
        }

        /// <summary>
        /// Gets all diagnosis recommendations for a maintenance record.
        /// This method retrieves the complete diagnosis history.
        /// 
        /// Process:
        /// 1. Finds all recommendations for record
        /// 2. Orders by creation date
        /// 3. Returns recommendation list
        /// </summary>
        /// <param name="maintenanceRecordId">The ID of the maintenance record</param>
        /// <returns>List of diagnosis recommendations ordered by date</returns>
        public List<DiagnosisRecommendation> GetDiagnosisRecommendations(int maintenanceRecordId)
        {
            return _context.DiagnosisRecommendations
                .Where(d => d.MaintenanceRecordId == maintenanceRecordId)
                .OrderByDescending(d => d.CreatedAt)
                .ToList();
        }

        /// <summary>
        /// Gets all pending diagnosis recommendations for a vehicle.
        /// This method retrieves incomplete recommendations.
        /// 
        /// Process:
        /// 1. Finds all recommendations for vehicle
        /// 2. Filters for incomplete recommendations
        /// 3. Orders by priority
        /// 4. Returns recommendation list
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle</param>
        /// <returns>List of pending diagnosis recommendations ordered by priority</returns>
        public List<DiagnosisRecommendation> GetPendingDiagnosisRecommendations(int vehicleId)
        {
            return _context.DiagnosisRecommendations
                .Where(d => d.MaintenanceRecord.VehicleId == vehicleId && !d.IsCompleted)
                .OrderByDescending(d => d.Priority)
                .ToList();
        }

        /// <summary>
        /// Updates an existing diagnosis recommendation.
        /// This method handles the complete recommendation update process.
        /// 
        /// Process:
        /// 1. Validates updated data
        /// 2. Finds existing recommendation
        /// 3. Updates recommendation
        /// 4. Saves changes
        /// 
        /// Validation:
        /// - All updated fields must be valid
        /// - Recommendation must exist
        /// </summary>
        /// <param name="recommendation">The diagnosis recommendation with updated information</param>
        /// <exception cref="ValidationException">Thrown when diagnosis data is invalid</exception>
        public void UpdateDiagnosisRecommendation(DiagnosisRecommendation recommendation)
        {
            var validationResult = _validationService.ValidateDiagnosisRecommendation(recommendation);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.GetErrorMessage());
            }

            var existingRecommendation = _context.DiagnosisRecommendations.Find(recommendation.Id);
            if (existingRecommendation != null)
            {
                _context.Entry(existingRecommendation).CurrentValues.SetValues(recommendation);
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// Deletes a diagnosis recommendation from the database.
        /// This method handles the complete recommendation removal process.
        /// 
        /// Process:
        /// 1. Finds recommendation by ID
        /// 2. Deletes recommendation
        /// 3. Updates maintenance record if needed
        /// 4. Saves changes
        /// 
        /// Note:
        /// This operation is permanent and cannot be undone.
        /// </summary>
        /// <param name="recommendationId">The ID of the diagnosis recommendation to delete</param>
        public void DeleteDiagnosisRecommendation(int recommendationId)
        {
            var recommendation = _context.DiagnosisRecommendations.Find(recommendationId);
            if (recommendation != null)
            {
                _context.DiagnosisRecommendations.Remove(recommendation);
                
                // Update maintenance record if no more recommendations
                var maintenanceRecord = _context.MaintenanceRecords
                    .Include(m => m.DiagnosisRecommendations)
                    .FirstOrDefault(m => m.Id == recommendation.MaintenanceRecordId);
                
                if (maintenanceRecord != null && !maintenanceRecord.DiagnosisRecommendations.Any())
                {
                    maintenanceRecord.HasDiagnosisRecommendations = false;
                }

                _context.SaveChanges();
            }
        }

        /// <summary>
        /// Requests a password reset for a user.
        /// This method handles the complete password reset request process.
        /// 
        /// Process:
        /// 1. Validates email
        /// 2. Generates reset token
        /// 3. Sends reset email
        /// 4. Saves changes
        /// 
        /// Security:
        /// - Reset token is time-limited
        /// - Email must be verified
        /// </summary>
        /// <param name="email">The email address of the user</param>
        /// <returns>True if request was successful, false otherwise</returns>
        public bool RequestPasswordReset(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                var resetToken = GenerateResetToken();
                user.PasswordResetToken = resetToken;
                user.PasswordResetExpiry = DateTime.Now.AddHours(24);
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Resets a user's password using a reset token.
        /// This method handles the complete password reset process.
        /// 
        /// Process:
        /// 1. Validates reset token
        /// 2. Verifies token expiry
        /// 3. Updates password
        /// 4. Clears reset token
        /// 5. Saves changes
        /// 
        /// Security:
        /// - Token must be valid
        /// - Token must not be expired
        /// - New password must be hashed
        /// </summary>
        /// <param name="email">The email address of the user</param>
        /// <param name="resetToken">The reset token received via email</param>
        /// <param name="newPassword">The new password to set</param>
        /// <returns>True if reset was successful, false otherwise</returns>
        public bool ResetPassword(string email, string resetToken, string newPassword)
        {
            var user = _context.Users.FirstOrDefault(u => 
                u.Email == email && 
                u.PasswordResetToken == resetToken && 
                u.PasswordResetExpiry > DateTime.Now);

            if (user != null)
            {
                user.Password = HashPassword(newPassword);
                user.PasswordResetToken = null;
                user.PasswordResetExpiry = null;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Generates a secure reset token for password reset.
        /// This method creates a cryptographically secure token.
        /// 
        /// Process:
        /// 1. Generates random bytes
        /// 2. Converts to hexadecimal string
        /// 3. Returns token
        /// 
        /// Security:
        /// - Uses cryptographically secure random number generator
        /// - Generates 32-byte token
        /// </summary>
        /// <returns>A secure reset token</returns>
        private string GenerateResetToken()
        {
            var tokenBytes = new byte[32];
            RandomNumberGenerator.Fill(tokenBytes);
            return BitConverter.ToString(tokenBytes).Replace("-", "");
        }

        /// <summary>
        /// Hashes a password using SHA256.
        /// This method provides secure password storage.
        /// 
        /// Process:
        /// 1. Converts password to bytes
        /// 2. Computes SHA256 hash
        /// 3. Converts to hexadecimal string
        /// 
        /// Security:
        /// - Uses SHA256 hashing algorithm
        /// - Converts to lowercase for consistency
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <returns>The hashed password</returns>
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        #endregion

        public List<Vehicle> GetAllVehicles()
        {
            return _context.Vehicles.ToList();
        }

        public bool AddVehicle(Vehicle vehicle)
        {
            try
            {
                var validationResult = _validationService.ValidateVehicle(vehicle);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.GetErrorMessage());
                }

                _context.Vehicles.Add(vehicle);
                _context.SaveChanges();
                return true;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteVehicle(int vehicleId)
        {
            try
            {
                var vehicle = _context.Vehicles.Find(vehicleId);
                if (vehicle == null)
                {
                    return false;
                }

                _context.Vehicles.Remove(vehicle);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<MaintenanceRecord> GetRecentMaintenanceRecords(int count)
        {
            using (var context = new AutoCareProContext())
            {
                return context.MaintenanceRecords
                    .Include(m => m.Vehicle)
                    .OrderByDescending(m => m.ServiceDate)
                    .Take(count)
                    .ToList();
            }
        }

        public List<Appointment> GetUpcomingAppointments(int maintenanceCenterId)
        {
            return _context.Appointments
                .Include(a => a.Vehicle)
                .Include(a => a.MaintenanceCenter)
                .Where(a => a.MaintenanceCenterId == maintenanceCenterId && 
                           a.AppointmentDate >= DateTime.Now)
                .OrderBy(a => a.AppointmentDate)
                .ToList();
        }

        public List<Appointment> GetAppointmentsByServiceProvider(int serviceProviderId)
        {
            return _context.Appointments
                .Include(a => a.Vehicle)
                .Include(a => a.MaintenanceCenter)
                .Where(a => a.ServiceProviderId == serviceProviderId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();
        }

        public List<Service> GetServicesByProvider(int serviceProviderId)
        {
            return _context.Services
                .Where(s => s.ServiceProviderId == serviceProviderId)
                .OrderBy(s => s.Name)
                .ToList();
        }

        public List<Review> GetReviewsByServiceProvider(int serviceProviderId)
        {
            return _context.Reviews
                .Include(r => r.Customer)
                .Where(r => r.ServiceProviderId == serviceProviderId)
                .OrderByDescending(r => r.Date)
                .ToList();
        }

        public void AddAppointment(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            _context.SaveChanges();
        }

        public void UpdateAppointment(Appointment appointment)
        {
            var existingAppointment = _context.Appointments.Find(appointment.Id);
            if (existingAppointment != null)
            {
                _context.Entry(existingAppointment).CurrentValues.SetValues(appointment);
                existingAppointment.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public void DeleteAppointment(int appointmentId)
        {
            var appointment = _context.Appointments.Find(appointmentId);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                _context.SaveChanges();
            }
        }

        public Appointment GetAppointmentById(int appointmentId)
        {
            return _context.Appointments
                .Include(a => a.Vehicle)
                .Include(a => a.MaintenanceCenter)
                .Include(a => a.ServiceProvider)
                .FirstOrDefault(a => a.Id == appointmentId);
        }

        public List<Appointment> GetAppointmentsByVehicle(int vehicleId)
        {
            return _context.Appointments
                .Include(a => a.Vehicle)
                .Include(a => a.ServiceProvider)
                .Where(a => a.VehicleId == vehicleId)
                .ToList();
        }

        public Service GetServiceById(int serviceId)
        {
            return _context.Services
                .Include(s => s.ServiceProvider)
                .Include(s => s.Bookings)
                .FirstOrDefault(s => s.Id == serviceId);
        }

        public void AddService(Service service)
        {
            _context.Services.Add(service);
            _context.SaveChanges();
        }

        public void UpdateService(Service service)
        {
            var existingService = _context.Services.Find(service.Id);
            if (existingService != null)
            {
                _context.Entry(existingService).CurrentValues.SetValues(service);
                _context.SaveChanges();
            }
        }

        public void DeleteService(int serviceId)
        {
            var service = _context.Services.Find(serviceId);
            if (service != null)
            {
                _context.Services.Remove(service);
                _context.SaveChanges();
            }
        }

        public Review GetReviewById(int reviewId)
        {
            return _context.Reviews
                .Include(r => r.ServiceProvider)
                .FirstOrDefault(r => r.Id == reviewId);
        }

        public void AddReview(Review review)
        {
            _context.Reviews.Add(review);
            _context.SaveChanges();
        }

        public void UpdateReview(Review review)
        {
            var existingReview = _context.Reviews.Find(review.Id);
            if (existingReview != null)
            {
                _context.Entry(existingReview).CurrentValues.SetValues(review);
                _context.SaveChanges();
            }
        }

        public void DeleteReview(int reviewId)
        {
            var review = _context.Reviews.Find(reviewId);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                _context.SaveChanges();
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

        ~DatabaseService()
        {
            Dispose(false);
        }

        public async Task<List<Service>> GetServicesByProviderIdAsync(int providerId)
        {
            return await _context.Services
                .Where(s => s.ProviderId == providerId)
                .ToListAsync();
        }

        public async Task<bool> DeleteServiceAsync(int serviceId)
        {
            try
            {
                var service = await _context.Services.FindAsync(serviceId);
                if (service != null)
                {
                    _context.Services.Remove(service);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddUserAsync(User user)
        {
            try
            {
                // Validate user data before registration
                var validationResult = _validationService.ValidateUser(user);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.GetErrorMessage());
                }

                // Check for duplicate username
                if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                {
                    return false;
                }

                // Check for duplicate email
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    return false;
                }

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user");
                return false;
            }
        }

        public Vehicle? GetVehicleById(int id)
        {
            return _context.Vehicles
                .Include(v => v.MaintenanceRecords)
                .FirstOrDefault(v => v.Id == id);
        }

        public async Task<bool> AddMaintenanceRecord(MaintenanceRecord record)
        {
            try
            {
                // Validate the record
                if (record == null)
                {
                    throw new ArgumentNullException(nameof(record));
                }

                // Check if the vehicle exists
                var vehicle = await _context.Vehicles.FindAsync(record.VehicleId);
                if (vehicle == null)
                {
                    throw new ValidationException("Vehicle not found");
                }

                await _context.MaintenanceRecords.AddAsync(record);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding maintenance record");
                return false;
            }
        }

        public async Task<bool> UpdateMaintenanceRecord(MaintenanceRecord record)
        {
            try
            {
                if (record == null)
                {
                    throw new ArgumentNullException(nameof(record));
                }

                var existingRecord = await _context.MaintenanceRecords.FindAsync(record.Id);
                if (existingRecord == null)
                {
                    throw new ValidationException("Maintenance record not found");
                }

                _context.Entry(existingRecord).CurrentValues.SetValues(record);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating maintenance record");
                return false;
            }
        }

        public async Task<List<MaintenanceRecord>> GetMaintenanceRecordsByVehicleId(int vehicleId)
        {
            return await _context.MaintenanceRecords
                .Where(r => r.VehicleId == vehicleId)
                .OrderByDescending(r => r.Date)
                .ToListAsync();
        }

        public async Task<MaintenanceRecord?> GetMaintenanceRecordById(int id)
        {
            return await _context.MaintenanceRecords
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }

    /// <summary>
    /// Custom exception class for validation errors.
    /// This is thrown when data validation fails.
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// Creates a new ValidationException with the specified message.
        /// </summary>
        /// <param name="message">The error message describing the validation failure</param>
        public ValidationException(string message) : base(message) { }
    }
}
