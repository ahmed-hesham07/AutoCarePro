using System; // Import the System namespace for basic types like DateTime
using System.Collections.Generic;

namespace AutoCarePro.Models
{
    /// <summary>
    /// MaintenanceRecord class represents a record of maintenance performed on a vehicle.
    /// This class serves as the primary entity for tracking vehicle maintenance history.
    /// 
    /// Key features:
    /// 1. Records detailed maintenance information (type, cost, service provider)
    /// 2. Tracks maintenance dates and vehicle mileage
    /// 3. Links maintenance to specific vehicles and service providers
    /// 4. Supports diagnosis recommendations
    /// 5. Maintains audit trail with creation and update timestamps
    /// 
    /// Usage:
    /// This class is used throughout the application to:
    /// - Track maintenance history
    /// - Generate maintenance reports
    /// - Calculate maintenance costs
    /// - Monitor vehicle service history
    /// </summary>
    public class MaintenanceRecord
    {
        /// <summary>
        /// Unique identifier for the maintenance record in the database.
        /// This is automatically generated when a new record is created.
        /// Used for database relationships and record identification.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to the Vehicle this maintenance record belongs to.
        /// This creates a relationship between the maintenance record
        /// and the specific vehicle in the database.
        /// </summary>
        public int VehicleId { get; set; }

        /// <summary>
        /// The date when the maintenance was performed.
        /// This is used for:
        /// - Maintenance history tracking
        /// - Service interval calculations
        /// - Maintenance reporting
        /// </summary>
        public DateTime MaintenanceDate { get; set; }

        /// <summary>
        /// The type of maintenance performed (e.g., Oil Change, Brake Service).
        /// This categorizes the maintenance work and is used for:
        /// - Maintenance history filtering
        /// - Service recommendations
        /// - Maintenance reporting
        /// </summary>
        public required string MaintenanceType { get; set; }

        /// <summary>
        /// Detailed description of the maintenance work performed.
        /// This should include:
        /// - Work performed
        /// - Parts replaced
        /// - Special procedures
        /// - Any issues found
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// The vehicle's mileage at the time of maintenance.
        /// This is crucial for:
        /// - Maintenance interval tracking
        /// - Service recommendations
        /// - Vehicle history documentation
        /// </summary>
        public int MileageAtMaintenance { get; set; }

        /// <summary>
        /// The cost of the maintenance in the system's currency.
        /// This is used for:
        /// - Cost tracking and analysis
        /// - Maintenance budgeting
        /// - Financial reporting
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// The name of the service provider or garage that performed the maintenance.
        /// This is used for:
        /// - Service provider tracking
        /// - Maintenance history documentation
        /// - Service quality assessment
        /// </summary>
        public required string ServiceProvider { get; set; }

        /// <summary>
        /// Additional notes or comments about the maintenance.
        /// This field can store:
        /// - Special instructions
        /// - Future recommendations
        /// - Important observations
        /// - Customer feedback
        /// </summary>
        public required string Notes { get; set; }

        /// <summary>
        /// Indicates whether this maintenance record has associated diagnosis recommendations.
        /// This is used to:
        /// - Track diagnosis status
        /// - Filter maintenance records
        /// - Generate maintenance reports
        /// </summary>
        public bool HasDiagnosisRecommendations { get; set; }

        /// <summary>
        /// Timestamp when this maintenance record was created.
        /// This is automatically set when a new record is created.
        /// Used for:
        /// - Audit trailing
        /// - Record tracking
        /// - System maintenance
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when this maintenance record was last updated.
        /// This is automatically updated whenever the record is modified.
        /// Used for:
        /// - Change tracking
        /// - Audit trailing
        /// - Data integrity
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Navigation property to the Vehicle associated with this maintenance record.
        /// This allows easy access to the vehicle's information
        /// when working with maintenance data.
        /// </summary>
        public required Vehicle Vehicle { get; set; }

        /// <summary>
        /// Collection of diagnosis recommendations associated with this maintenance record.
        /// This allows tracking of all diagnoses and recommendations made during maintenance.
        /// </summary>
        public List<DiagnosisRecommendation> DiagnosisRecommendations { get; set; }

        /// <summary>
        /// Indicates whether the maintenance work has been completed.
        /// This is used for:
        /// - Tracking maintenance status
        /// - Filtering maintenance records
        /// - Generating maintenance reports
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Foreign key to the User who diagnosed the maintenance issue.
        /// This creates a relationship between the maintenance record
        /// and the user who performed the diagnosis.
        /// </summary>
        public int? DiagnosedByUserId { get; set; }

        public MaintenanceRecord()
        {
            DiagnosisRecommendations = new List<DiagnosisRecommendation>();
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
            IsCompleted = false;
        }
    }
} 