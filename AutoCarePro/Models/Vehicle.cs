using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoCarePro.Models
{
    /// <summary>
    /// Vehicle class represents a vehicle in the AutoCarePro system.
    /// This class serves as the central entity for vehicle management and maintenance tracking.
    /// 
    /// Key features:
    /// 1. Stores comprehensive vehicle information (make, model, specifications)
    /// 2. Tracks maintenance history and recommendations
    /// 3. Manages vehicle ownership and relationships
    /// 4. Records important dates and milestones
    /// 5. Maintains vehicle identification details
    /// 
    /// Usage:
    /// This class is used throughout the application to:
    /// - Display vehicle information in the dashboard
    /// - Track maintenance schedules
    /// - Generate maintenance reports
    /// - Manage vehicle ownership
    /// </summary>
    public class Vehicle
    {
        /// <summary>
        /// Unique identifier for the vehicle in the database.
        /// This is automatically generated when a new vehicle is registered.
        /// Used for database relationships and record identification.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Manufacturer of the vehicle (e.g., Toyota, Honda, Ford).
        /// This is used for:
        /// - Vehicle categorization
        /// - Maintenance recommendations
        /// - Parts compatibility
        /// </summary>
        public required string Make { get; set; }

        /// <summary>
        /// Specific model of the vehicle (e.g., Camry, Civic, F-150).
        /// Used in combination with Make to identify the exact vehicle type.
        /// Important for:
        /// - Maintenance scheduling
        /// - Parts ordering
        /// - Service recommendations
        /// </summary>
        public required string Model { get; set; }

        /// <summary>
        /// Year the vehicle was manufactured.
        /// This is crucial for:
        /// - Determining maintenance schedules
        /// - Identifying compatible parts
        /// - Calculating vehicle age
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Vehicle Identification Number (VIN) - unique serial number for the vehicle.
        /// This 17-character code contains:
        /// - Manufacturer information
        /// - Vehicle specifications
        /// - Production details
        /// Used for:
        /// - Vehicle identification
        /// - Registration
        /// - Maintenance tracking
        /// </summary>
        public required string VIN { get; set; }

        /// <summary>
        /// Current mileage of the vehicle in kilometers/miles.
        /// This is used for:
        /// - Maintenance scheduling
        /// - Service recommendations
        /// - Vehicle value assessment
        /// Should be updated regularly for accurate maintenance tracking.
        /// </summary>
        public int CurrentMileage { get; set; }

        /// <summary>
        /// Date of the last maintenance performed on the vehicle.
        /// Used to:
        /// - Track maintenance intervals
        /// - Generate maintenance reminders
        /// - Calculate next service date
        /// </summary>
        public DateTime? LastMaintenanceDate { get; set; }

        /// <summary>
        /// Collection of all maintenance records for this vehicle.
        /// Each record contains:
        /// - Service details
        /// - Maintenance type
        /// - Cost information
        /// - Service provider
        /// Used for maintenance history and reporting.
        /// </summary>
        public List<MaintenanceRecord> MaintenanceHistory { get; set; }

        /// <summary>
        /// Collection of maintenance recommendations for this vehicle.
        /// These are generated based on:
        /// - Maintenance history
        /// - Vehicle age
        /// - Current mileage
        /// - Manufacturer guidelines
        /// </summary>
        public List<MaintenanceRecommendation> Recommendations { get; set; }

        /// <summary>
        /// Foreign key to the User who owns this vehicle.
        /// This creates a relationship between the vehicle and its owner
        /// in the database.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Navigation property to the User who owns this vehicle.
        /// This allows easy access to the owner's information
        /// when working with vehicle data.
        /// </summary>
        public required User User { get; set; }

        /// <summary>
        /// Vehicle's license plate number.
        /// This is used for:
        /// - Vehicle identification
        /// - Registration
        /// - Legal compliance
        /// </summary>
        public required string LicensePlate { get; set; }

        /// <summary>
        /// Type of fuel the vehicle uses (e.g., Gasoline, Diesel, Electric).
        /// This information is important for:
        /// - Maintenance scheduling
        /// - Service recommendations
        /// - Environmental impact tracking
        /// </summary>
        public required string FuelType { get; set; }

        /// <summary>
        /// Date when the vehicle was purchased.
        /// Used for:
        /// - Warranty tracking
        /// - Vehicle age calculation
        /// - Maintenance scheduling
        /// </summary>
        public DateTime PurchaseDate { get; set; }

        /// <summary>
        /// Additional notes or comments about the vehicle.
        /// This field can store:
        /// - Special modifications
        /// - Known issues
        /// - Important reminders
        /// - Custom maintenance notes
        /// </summary>
        public required string Notes { get; set; }

        /// <summary>
        /// Vehicle type (e.g., Car, Truck, SUV, Van, Motorcycle).
        /// </summary>
        public VehicleType Type { get; set; }

        /// <summary>
        /// Indicates whether the vehicle needs maintenance based on:
        /// - Last maintenance date
        /// - Current mileage
        /// - Maintenance recommendations
        /// </summary>
        public bool NeedsMaintenance
        {
            get
            {
                if (!LastMaintenanceDate.HasValue)
                    return true;

                var daysSinceLastMaintenance = (DateTime.Now - LastMaintenanceDate.Value).TotalDays;
                var mileageSinceLastMaintenance = CurrentMileage - (MaintenanceHistory.LastOrDefault()?.Mileage ?? 0);

                // Check if maintenance is needed based on time (6 months) or mileage (5000 km)
                return daysSinceLastMaintenance >= 180 || mileageSinceLastMaintenance >= 5000;
            }
        }

        /// <summary>
        /// Constructor for creating a new Vehicle instance.
        /// This initializes the vehicle with default values:
        /// - Creates empty lists for maintenance history and recommendations
        /// 
        /// Usage:
        /// var newVehicle = new Vehicle();
        /// </summary>
        public Vehicle()
        {
            // Initialize empty lists to prevent null reference exceptions
            MaintenanceHistory = new List<MaintenanceRecord>();
            Recommendations = new List<MaintenanceRecommendation>();
        }
    }

    public enum VehicleType
    {
        Car,
        Truck,
        SUV,
        Van,
        Motorcycle
    }
} 