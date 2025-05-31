using System;
using System.Text.RegularExpressions;
using AutoCarePro.Models;

namespace AutoCarePro.Services
{
    /// <summary>
    /// Service class responsible for validating data across the application.
    /// Provides methods for validating user input, ensuring data integrity,
    /// and maintaining consistent validation rules throughout the application.
    /// </summary>
    public class DataValidationService
    {
        // Regular expressions for validation
        private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        private static readonly Regex PhoneRegex = new Regex(@"^\+?[\d\s-]{10,}$");
        private static readonly Regex PasswordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");

        /// <summary>
        /// Validates user data including email, phone number, and other required fields.
        /// </summary>
        /// <param name="user">The user object to validate</param>
        /// <returns>A ValidationResult object containing validation status and any error messages</returns>
        public ValidationResult ValidateUser(User user)
        {
            var result = new ValidationResult();

            // Validate email
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                result.AddError("Email is required.");
            }
            else if (!EmailRegex.IsMatch(user.Email))
            {
                result.AddError("Invalid email format.");
            }

            // Validate full name
            if (string.IsNullOrWhiteSpace(user.FullName))
            {
                result.AddError("Full name is required.");
            }
            else if (user.FullName.Length < 2)
            {
                result.AddError("Full name must be at least 2 characters long.");
            }

            // Validate phone number
            if (!string.IsNullOrWhiteSpace(user.PhoneNumber) && !PhoneRegex.IsMatch(user.PhoneNumber))
            {
                result.AddError("Invalid phone number format.");
            }

            return result;
        }

        /// <summary>
        /// Validates a password against security requirements.
        /// </summary>
        /// <param name="password">The password to validate</param>
        /// <returns>A ValidationResult object containing validation status and any error messages</returns>
        public ValidationResult ValidatePassword(string password)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(password))
            {
                result.AddError("Password is required.");
            }
            else if (!PasswordRegex.IsMatch(password))
            {
                result.AddError("Password must be at least 8 characters long and contain uppercase, lowercase, number, and special character.");
            }

            return result;
        }

        /// <summary>
        /// Validates vehicle data including make, model, and other required fields.
        /// </summary>
        /// <param name="vehicle">The vehicle object to validate</param>
        /// <returns>A ValidationResult object containing validation status and any error messages</returns>
        public ValidationResult ValidateVehicle(Vehicle vehicle)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(vehicle.Make))
            {
                result.AddError("Vehicle make is required.");
            }

            if (string.IsNullOrWhiteSpace(vehicle.Model))
            {
                result.AddError("Vehicle model is required.");
            }

            if (vehicle.Year < 1900 || vehicle.Year > DateTime.Now.Year + 1)
            {
                result.AddError($"Vehicle year must be between 1900 and {DateTime.Now.Year + 1}.");
            }

            if (string.IsNullOrWhiteSpace(vehicle.LicensePlate))
            {
                result.AddError("License plate is required.");
            }

            return result;
        }

        /// <summary>
        /// Validates maintenance record data including type, description, and other required fields.
        /// </summary>
        /// <param name="record">The maintenance record object to validate</param>
        /// <returns>A ValidationResult object containing validation status and any error messages</returns>
        public ValidationResult ValidateMaintenanceRecord(MaintenanceRecord record)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(record.MaintenanceType))
            {
                result.AddError("Maintenance type is required.");
            }

            if (string.IsNullOrWhiteSpace(record.Description))
            {
                result.AddError("Description is required.");
            }

            if (record.MaintenanceDate > DateTime.Now)
            {
                result.AddError("Maintenance date cannot be in the future.");
            }

            if (record.Cost < 0)
            {
                result.AddError("Cost cannot be negative.");
            }

            return result;
        }
    }
} 