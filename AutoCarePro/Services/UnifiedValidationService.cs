using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AutoCarePro.Models;
using System.Collections.Generic;

namespace AutoCarePro.Services
{
    /// <summary>
    /// UnifiedValidationService provides comprehensive validation functionality for the AutoCarePro application.
    /// This service combines both UI input validation and data model validation into a single, cohesive service.
    /// </summary>
    public class UnifiedValidationService
    {
        // Regular expressions for validation
        private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        private static readonly Regex PhoneRegex = new Regex(@"^\+?[0-9]{10,15}$");
        private static readonly Regex VINRegex = new Regex(@"^[A-HJ-NPR-Z0-9]{17}$");

        #region UI Input Validation

        /// <summary>
        /// Validates that a required field is not empty or whitespace.
        /// </summary>
        public bool ValidateRequired(string value, string fieldName, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errorMessage = $"{fieldName} is required.";
                return false;
            }
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Validates an email address format.
        /// </summary>
        public bool ValidateEmail(string email, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                errorMessage = "Email is required.";
                return false;
            }

            if (!EmailRegex.IsMatch(email))
            {
                errorMessage = "Please enter a valid email address.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Validates a phone number format.
        /// </summary>
        public bool ValidatePhone(string phone, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                errorMessage = "Phone number is required.";
                return false;
            }

            if (!PhoneRegex.IsMatch(phone))
            {
                errorMessage = "Please enter a valid phone number (10-15 digits).";
                return false;
            }

            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Validates that a string value can be parsed as a number.
        /// </summary>
        public bool ValidateNumeric(string value, string fieldName, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errorMessage = $"{fieldName} is required.";
                return false;
            }

            if (!decimal.TryParse(value, out _))
            {
                errorMessage = $"{fieldName} must be a valid number.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Validates a date to ensure it is not in the past.
        /// </summary>
        public bool ValidateDate(DateTime? date, string fieldName, out string errorMessage)
        {
            if (!date.HasValue)
            {
                errorMessage = $"{fieldName} is required.";
                return false;
            }

            if (date.Value < DateTime.Now.Date)
            {
                errorMessage = $"{fieldName} cannot be in the past.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Displays a validation error message to the user.
        /// </summary>
        public void ShowValidationError(string message)
        {
            MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        #endregion

        #region Data Model Validation

        /// <summary>
        /// Validates user registration and profile data
        /// </summary>
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
        public ValidationResult ValidateMaintenanceRecord(MaintenanceRecord record)
        {
            var result = new ValidationResult();

            // Validate required fields
            if (record.ServiceDate == default)
            {
                result.AddError("Service date is required");
            }

            if (record.Mileage <= 0)
            {
                result.AddError("Mileage must be greater than 0");
            }

            if (string.IsNullOrWhiteSpace(record.MaintenanceType))
            {
                result.AddError("Maintenance type is required");
            }

            if (string.IsNullOrWhiteSpace(record.Description))
            {
                result.AddError("Description is required");
            }

            if (record.Cost <= 0)
            {
                result.AddError("Cost must be greater than 0");
            }

            return result;
        }

        /// <summary>
        /// Validates maintenance recommendation data
        /// </summary>
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
        public ValidationResult ValidateDiagnosisRecommendation(DiagnosisRecommendation recommendation)
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

            return result;
        }

        /// <summary>
        /// Validates service data
        /// </summary>
        public ValidationResult ValidateService(Service service)
        {
            var result = new ValidationResult();

            // Validate name
            if (string.IsNullOrWhiteSpace(service.Name))
                result.AddError("Service name is required");
            else if (service.Name.Length > 100)
                result.AddError("Service name must not exceed 100 characters");

            // Validate description
            if (string.IsNullOrWhiteSpace(service.Description))
                result.AddError("Description is required");
            else if (service.Description.Length > 500)
                result.AddError("Description must not exceed 500 characters");

            // Validate price
            if (service.Price < 0)
                result.AddError("Price cannot be negative");

            // Validate duration
            if (service.DurationMinutes <= 0)
                result.AddError("Duration must be greater than 0");

            // Validate category
            if (string.IsNullOrWhiteSpace(service.Category))
                result.AddError("Category is required");
            else if (service.Category.Length > 50)
                result.AddError("Category must not exceed 50 characters");

            return result;
        }

        /// <summary>
        /// Validates review data
        /// </summary>
        public ValidationResult ValidateReview(Review review)
        {
            var result = new ValidationResult();

            // Validate rating
            if (review.Rating < 1 || review.Rating > 5)
                result.AddError("Rating must be between 1 and 5");

            // Validate comment
            if (string.IsNullOrWhiteSpace(review.Comment))
                result.AddError("Comment is required");
            else if (review.Comment.Length > 1000)
                result.AddError("Comment must not exceed 1000 characters");

            return result;
        }

        #endregion
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
        public void AddError(string error)
        {
            Errors.Add(error);
        }

        /// <summary>
        /// Gets all error messages as a single string, separated by newlines
        /// </summary>
        public string GetErrorMessage()
        {
            return string.Join(Environment.NewLine, Errors);
        }
    }
} 