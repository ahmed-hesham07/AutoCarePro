using System; // Import the System namespace for basic types like DateTime

namespace AutoCarePro.Models
{
    /// <summary>
    /// DiagnosisRecommendation class represents a recommendation made during a vehicle diagnosis.
    /// This class serves as the primary entity for managing vehicle diagnosis recommendations.
    /// 
    /// Key features:
    /// 1. Tracks diagnosis recommendations from maintenance centers
    /// 2. Manages component-specific issues and solutions
    /// 3. Prioritizes maintenance needs based on severity
    /// 4. Tracks cost estimates and completion status
    /// 5. Maintains audit trail with creation and update timestamps
    /// 
    /// Usage:
    /// This class is used throughout the application to:
    /// - Record diagnosis findings
    /// - Track recommended repairs
    /// - Monitor repair completion
    /// - Manage maintenance costs
    /// </summary>
    public class DiagnosisRecommendation
    {
        /// <summary>
        /// Unique identifier for the diagnosis recommendation in the database.
        /// This is automatically generated when a new recommendation is created.
        /// Used for database relationships and record identification.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to the MaintenanceRecord this recommendation is linked to.
        /// This creates a relationship between the diagnosis recommendation
        /// and the specific maintenance record in the database.
        /// </summary>
        public int MaintenanceRecordId { get; set; }

        /// <summary>
        /// Foreign key to the User (maintenance center) who made the diagnosis.
        /// This tracks which maintenance center provided the diagnosis
        /// and is used for accountability and follow-up.
        /// </summary>
        public int DiagnosedByUserId { get; set; }

        /// <summary>
        /// The component of the vehicle that needs attention.
        /// This is used for:
        /// - Component-specific issue tracking
        /// - Maintenance categorization
        /// - Parts ordering
        /// - Service planning
        /// </summary>
        public required string Component { get; set; }

        /// <summary>
        /// Detailed description of the issue or recommendation.
        /// This should include:
        /// - Problem description
        /// - Required repairs
        /// - Potential consequences
        /// - Special considerations
        /// - Recommended solutions
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// Priority level of the recommendation (Critical, High, Medium, or Low).
        /// This determines:
        /// - Repair urgency
        /// - Scheduling priority
        /// - Notification frequency
        /// - Resource allocation
        /// </summary>
        public PriorityLevel Priority { get; set; }

        /// <summary>
        /// Estimated cost to fix the issue in the system's currency.
        /// This is used for:
        /// - Cost tracking and analysis
        /// - Budget planning
        /// - Financial reporting
        /// - Customer communication
        /// </summary>
        public decimal EstimatedCost { get; set; }

        /// <summary>
        /// The date by which the work should be done.
        /// This is determined based on:
        /// - Issue severity
        /// - Vehicle safety
        /// - Component criticality
        /// - Available resources
        /// </summary>
        public DateTime RecommendedDate { get; set; }

        /// <summary>
        /// Indicates whether the recommendation has been completed.
        /// This is used to:
        /// - Track repair status
        /// - Generate completion reports
        /// - Update maintenance history
        /// - Manage follow-up actions
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Timestamp when this recommendation was created.
        /// This is automatically set when a new recommendation is created.
        /// Used for:
        /// - Audit trailing
        /// - Age tracking
        /// - System maintenance
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when this recommendation was last updated.
        /// This is automatically updated whenever the record is modified.
        /// Used for:
        /// - Change tracking
        /// - Audit trailing
        /// - Data integrity
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Navigation property to the MaintenanceRecord associated with this recommendation.
        /// This allows easy access to the maintenance record's information
        /// when working with diagnosis data.
        /// </summary>
        public required MaintenanceRecord MaintenanceRecord { get; set; }

        /// <summary>
        /// Navigation property to the User (maintenance center) who made the diagnosis.
        /// This allows access to the diagnosing maintenance center's information
        /// when working with diagnosis data.
        /// </summary>
        public required User DiagnosedByUser { get; set; }

        /// <summary>
        /// Additional notes or comments related to the recommendation.
        /// </summary>
        public required string Notes { get; set; }

        /// <summary>
        /// The recommended action based on the diagnosis.
        /// </summary>
        public required string RecommendedAction { get; set; }
    }
} 