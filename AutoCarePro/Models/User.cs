using System;
using System.Collections.Generic;

namespace AutoCarePro.Models
{
    /// <summary>
    /// User class represents a user account in the AutoCarePro system.
    /// This class serves as the foundation for user management and authentication.
    /// 
    /// Key features:
    /// 1. Supports two types of users: Car Owners and Maintenance Centers
    /// 2. Stores essential user information (username, email, contact details)
    /// 3. Manages user authentication credentials
    /// 4. Tracks user activity and account creation
    /// 5. Maintains relationships with vehicles
    /// 
    /// Security considerations:
    /// - Passwords should always be stored in hashed form
    /// - Sensitive data should be encrypted
    /// - Access control based on user type
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique identifier for the user in the database.
        /// This is automatically generated when a new user is created.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Unique username used for login and identification.
        /// This must be unique across all users in the system.
        /// </summary>
        public required string Username { get; set; }

        /// <summary>
        /// Password used for user authentication.
        /// Important: This should always be stored in hashed form, never as plain text.
        /// The actual password hashing is handled by the authentication service.
        /// </summary>
        public required string Password { get; set; }

        /// <summary>
        /// User's email address used for:
        /// - Account recovery
        /// - Important notifications
        /// - System communications
        /// Must be a valid email format.
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// User's complete name for identification and display purposes.
        /// This is used in the user interface and reports.
        /// </summary>
        public required string FullName { get; set; }

        /// <summary>
        /// User's contact phone number for:
        /// - Emergency contact
        /// - Service notifications
        /// - Account verification
        /// Optional but recommended for better service.
        /// </summary>
        public required string PhoneNumber { get; set; }

        /// <summary>
        /// Defines the type of user account, which determines:
        /// - Available features and permissions
        /// - Interface customization
        /// - Data access rights
        /// See UserType enum for available options.
        /// </summary>
        public UserType Type { get; set; }

        /// <summary>
        /// Collection of vehicles associated with this user.
        /// For Car Owners: List of vehicles they own
        /// For Maintenance Centers: List of vehicles they service
        /// This relationship is managed by the database.
        /// </summary>
        public List<Vehicle> Vehicles { get; set; }

        /// <summary>
        /// Timestamp when the user account was created.
        /// This is automatically set when a new user is registered.
        /// Used for account age verification and auditing.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Timestamp of the user's most recent login.
        /// Updated automatically on each successful login.
        /// Used for:
        /// - Activity tracking
        /// - Security monitoring
        /// - Session management
        /// </summary>
        public DateTime LastLoginDate { get; set; }

        /// <summary>
        /// Token used for password reset functionality.
        /// This is generated when a user requests a password reset.
        /// </summary>
        public string? PasswordResetToken { get; set; }

        /// <summary>
        /// Expiration date for the password reset token.
        /// After this date, the token becomes invalid.
        /// </summary>
        public DateTime? PasswordResetExpiry { get; set; }

        /// <summary>
        /// Constructor for creating a new User instance.
        /// This initializes the user with default values:
        /// - Creates an empty list for vehicles
        /// - Sets the creation date to current time
        /// 
        /// Usage:
        /// var newUser = new User();
        /// </summary>
        public User()
        {
            // Initialize empty list of vehicles to prevent null reference exceptions
            Vehicles = new List<Vehicle>();
            // Set creation date to current date and time for tracking
            CreatedDate = DateTime.Now;
        }
    }

    /// <summary>
    /// Enumeration defining the possible types of users in the system.
    /// This determines the user's role and available permissions.
    /// 
    /// Available types:
    /// - CarOwner: Regular user who owns and manages vehicles
    /// - MaintenanceCenter: Service provider who performs maintenance
    /// - Admin: System administrator who can manage all users and system features
    /// 
    /// Usage:
    /// UserType type = UserType.CarOwner;
    /// </summary>
    public enum UserType
    {
        /// <summary>
        /// Regular car owner who can:
        /// - Register and manage their vehicles
        /// - View maintenance history
        /// - Receive maintenance recommendations
        /// </summary>
        CarOwner,

        /// <summary>
        /// Service center that can:
        /// - Perform maintenance on vehicles
        /// - Add maintenance records
        /// - Provide diagnoses and recommendations
        /// </summary>
        MaintenanceCenter,

        /// <summary>
        /// System administrator who can:
        /// - Manage all users
        /// - Access all system features
        /// - Configure system settings
        /// </summary>
        Admin
    }
} 