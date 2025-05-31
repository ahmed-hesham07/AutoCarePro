using System; // Import the System namespace for basic types like DateTime
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to the Vehicle this maintenance record belongs to.
        /// This creates a relationship between the maintenance record
        /// and the specific vehicle in the database.
        /// </summary>
        [Required]
        public int VehicleId { get; set; }

        /// <summary>
        /// Foreign key to the Service Provider this maintenance record belongs to.
        /// This creates a relationship between the maintenance record
        /// and the specific service provider in the database.
        /// </summary>
        [Required]
        public int ServiceProviderId { get; set; }

        /// <summary>
        /// The date when the maintenance was performed.
        /// This is used for:
        /// - Maintenance history tracking
        /// - Service interval calculations
        /// - Maintenance reporting
        /// </summary>
        [Required]
        public DateTime ServiceDate { get; set; }

        /// <summary>
        /// Alias for ServiceDate to maintain compatibility
        /// </summary>
        [NotMapped]
        public DateTime MaintenanceDate
        {
            get => ServiceDate;
            set => ServiceDate = value;
        }

        /// <summary>
        /// The type of maintenance performed (e.g., Oil Change, Brake Service).
        /// This categorizes the maintenance work and is used for:
        /// - Maintenance history filtering
        /// - Service recommendations
        /// - Maintenance reporting
        /// </summary>
        [Required]
        [StringLength(50)]
        public string MaintenanceType { get; set; }

        /// <summary>
        /// Detailed description of the maintenance work performed.
        /// This should include:
        /// - Work performed
        /// - Parts replaced
        /// - Special procedures
        /// - Any issues found
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// The vehicle's mileage at the time of maintenance.
        /// This is crucial for:
        /// - Maintenance interval tracking
        /// - Service recommendations
        /// - Vehicle history documentation
        /// </summary>
        [Required]
        public int Mileage { get; set; }

        /// <summary>
        /// Alias for Mileage to maintain compatibility
        /// </summary>
        [NotMapped]
        public int MileageAtMaintenance
        {
            get => Mileage;
            set => Mileage = value;
        }

        /// <summary>
        /// The cost of the maintenance in the system's currency.
        /// This is used for:
        /// - Cost tracking and analysis
        /// - Maintenance budgeting
        /// - Financial reporting
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }

        /// <summary>
        /// Additional notes or comments about the maintenance.
        /// This field can store:
        /// - Special instructions
        /// - Future recommendations
        /// - Important observations
        /// - Customer feedback
        /// </summary>
        [StringLength(1000)]
        public string Notes { get; set; }

        /// <summary>
        /// Timestamp when this maintenance record was created.
        /// This is automatically set when a new record is created.
        /// Used for:
        /// - Audit trailing
        /// - Record tracking
        /// - System maintenance
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp when this maintenance record was last updated.
        /// This is automatically updated whenever the record is modified.
        /// Used for:
        /// - Change tracking
        /// - Audit trailing
        /// - Data integrity
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Navigation property to the Vehicle associated with this maintenance record.
        /// This allows easy access to the vehicle's information
        /// when working with maintenance data.
        /// </summary>
        [ForeignKey("VehicleId")]
        public Vehicle Vehicle { get; set; }

        /// <summary>
        /// Navigation property to the Service Provider associated with this maintenance record.
        /// This allows easy access to the service provider's information
        /// when working with maintenance data.
        /// </summary>
        [ForeignKey("ServiceProviderId")]
        public User ServiceProvider { get; set; }

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

        /// <summary>
        /// Property to check if there are any diagnosis recommendations
        /// </summary>
        [NotMapped]
        public bool HasDiagnosisRecommendations => DiagnosisRecommendations?.Count > 0;

        public MaintenanceRecord()
        {
            DiagnosisRecommendations = new List<DiagnosisRecommendation>();
            IsCompleted = false;
        }
    }
} 