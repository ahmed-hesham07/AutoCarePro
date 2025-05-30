using System;
using System.Text.RegularExpressions;
using AutoCarePro.Models;

namespace AutoCarePro.Services
{
    /// <summary>
    /// DataValidationService class handles all data validation for the AutoCarePro application.
    /// This includes validation of user data, vehicle information, and maintenance records.
    /// </summary>
    public class DataValidationService
    {
        // Regular expression for validating email addresses
        private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        // Regular expression for validating phone numbers (10-15 digits, optional + prefix)
        private static readonly Regex PhoneRegex = new Regex(@"^\+?[0-9]{10,15}$");
        // Regular expression for validating Vehicle Identification Numbers (17 characters, excluding I, O, Q)
        private static readonly Regex VINRegex = new Regex(@"^[A-HJ-NPR-Z0-9]{17}$");

        /// <summary>
        /// Validates user registration and profile data
        /// </summary>
        /// <param name="user">The user object to validate</param>
        /// <returns>ValidationResult containing any validation errors</returns>
        public ValidationResult ValidateUser(User user)
        {
            var result = new ValidationResult();

            // Validate username
            if (string.IsNullOrWhiteSpace(user.Username))
                result.AddError("Username is required");
            else if (user.Username.Length < 3 || user.Username.Length > 50)
                result.AddError("Username must be between 3 and 50 characters");

            // Validate email
            if (string.IsNullOrWhiteSpace(user.Email))
                result.AddError("Email is required");
            else if (!EmailRegex.IsMatch(user.Email))
                result.AddError("Invalid email format");

            // Validate full name
            if (string.IsNullOrWhiteSpace(user.FullName))
                result.AddError("Full name is required");
            else if (user.FullName.Length > 100)
                result.AddError("Full name must not exceed 100 characters");

            // Validate phone number (optional)
            if (!string.IsNullOrWhiteSpace(user.PhoneNumber) && !PhoneRegex.IsMatch(user.PhoneNumber))
                result.AddError("Invalid phone number format");

            // Validate password
            if (string.IsNullOrWhiteSpace(user.Password))
                result.AddError("Password is required");
            else if (user.Password.Length < 8)
                result.AddError("Password must be at least 8 characters long");

            return result;
        }

        /// <summary>
        /// Validates vehicle information
        /// </summary>
        /// <param name="vehicle">The vehicle object to validate</param>
        /// <returns>ValidationResult containing any validation errors</returns>
        public ValidationResult ValidateVehicle(Vehicle vehicle)
        {
            var result = new ValidationResult();

            // Validate make
            if (string.IsNullOrWhiteSpace(vehicle.Make))
                result.AddError("Make is required");
            else if (vehicle.Make.Length > 50)
                result.AddError("Make must not exceed 50 characters");

            // Validate model
            if (string.IsNullOrWhiteSpace(vehicle.Model))
                result.AddError("Model is required");
            else if (vehicle.Model.Length > 50)
                result.AddError("Model must not exceed 50 characters");

            // Validate year
            if (vehicle.Year < 1900 || vehicle.Year > DateTime.Now.Year + 1)
                result.AddError($"Year must be between 1900 and {DateTime.Now.Year + 1}");

            // Validate VIN (optional)
            if (!string.IsNullOrWhiteSpace(vehicle.VIN) && !VINRegex.IsMatch(vehicle.VIN))
                result.AddError("Invalid VIN format");

            // Validate current mileage
            if (vehicle.CurrentMileage < 0)
                result.AddError("Current mileage cannot be negative");

            return result;
        }

        /// <summary>
        /// Validates maintenance record data
        /// </summary>
        /// <param name="record">The maintenance record to validate</param>
        /// <returns>ValidationResult containing any validation errors</returns>
        public ValidationResult ValidateMaintenanceRecord(MaintenanceRecord record)
        {
            var result = new ValidationResult();

            // Validate description
            if (string.IsNullOrWhiteSpace(record.Description))
                result.AddError("Description is required");
            else if (record.Description.Length > 500)
                result.AddError("Description must not exceed 500 characters");

            // Validate maintenance date
            if (record.MaintenanceDate > DateTime.Now)
                result.AddError("Maintenance date cannot be in the future");

            // Validate cost
            if (record.Cost < 0)
                result.AddError("Cost cannot be negative");

            // Validate mileage at maintenance
            if (record.MileageAtMaintenance < 0)
                result.AddError("Mileage at maintenance cannot be negative");

            return result;
        }

        /// <summary>
        /// Validates maintenance recommendation data
        /// </summary>
        /// <param name="recommendation">The maintenance recommendation to validate</param>
        /// <returns>ValidationResult containing any validation errors</returns>
        public ValidationResult ValidateMaintenanceRecommendation(MaintenanceRecommendation recommendation)
        {
            var result = new ValidationResult();

            // Validate description
            if (string.IsNullOrWhiteSpace(recommendation.Description))
                result.AddError("Description is required");
            else if (recommendation.Description.Length > 500)
                result.AddError("Description must not exceed 500 characters");

            // Validate priority
            if (recommendation.Priority < PriorityLevel.Low || recommendation.Priority > PriorityLevel.Critical)
                result.AddError("Invalid priority level");

            // Validate recommended date
            if (recommendation.RecommendedDate < DateTime.Now)
                result.AddError("Recommended date cannot be in the past");

            return result;
        }

        /// <summary>
        /// Validates diagnosis recommendation data
        /// </summary>
        /// <param name="recommendation">The diagnosis recommendation to validate</param>
        /// <returns>ValidationResult containing any validation errors</returns>
        public ValidationResult ValidateDiagnosisRecommendation(DiagnosisRecommendation recommendation)
        {
            var result = new ValidationResult();

            // Validate component
            if (string.IsNullOrWhiteSpace(recommendation.Component))
                result.AddError("Component is required");
            else if (recommendation.Component.Length > 100)
                result.AddError("Component must not exceed 100 characters");

            // Validate description
            if (string.IsNullOrWhiteSpace(recommendation.Description))
                result.AddError("Description is required");
            else if (recommendation.Description.Length > 1000)
                result.AddError("Description must not exceed 1000 characters");

            // Validate priority
            if (recommendation.Priority < PriorityLevel.Low || recommendation.Priority > PriorityLevel.Critical)
                result.AddError("Invalid priority level");

            // Validate estimated cost
            if (recommendation.EstimatedCost < 0)
                result.AddError("Estimated cost cannot be negative");

            return result;
        }
    }

    /// <summary>
    /// ValidationResult class represents the result of a validation operation.
    /// It contains a list of validation errors and methods to manage them.
    /// </summary>
    public class ValidationResult
    {
        // Indicates whether the validation was successful (no errors)
        public bool IsValid => Errors.Count == 0;
        // List of validation error messages
        public List<string> Errors { get; } = new List<string>();

        /// <summary>
        /// Adds an error message to the validation result
        /// </summary>
        /// <param name="error">The error message to add</param>
        public void AddError(string error)
        {
            Errors.Add(error);
        }

        /// <summary>
        /// Gets all error messages as a single string, separated by newlines
        /// </summary>
        /// <returns>A string containing all error messages</returns>
        public string GetErrorMessage()
        {
            return string.Join(Environment.NewLine, Errors);
        }
    }
} 