using System;

namespace AutoCarePro.Models
{
    /// <summary>
    /// MaintenanceRecommendation class represents a recommended maintenance task for a vehicle.
    /// This class serves as the primary entity for managing maintenance recommendations and scheduling.
    /// 
    /// Key features:
    /// 1. Tracks recommended maintenance tasks based on various factors
    /// 2. Manages maintenance scheduling by date and mileage
    /// 3. Prioritizes maintenance tasks based on urgency
    /// 4. Supports maintenance acknowledgment tracking
    /// 5. Links recommendations to specific vehicles
    /// 
    /// Usage:
    /// This class is used throughout the application to:
    /// - Generate maintenance schedules
    /// - Track maintenance priorities
    /// - Monitor maintenance acknowledgments
    /// - Plan future maintenance
    /// </summary>
    public class MaintenanceRecommendation
    {
        /// <summary>
        /// Unique identifier for the recommendation in the database.
        /// This is automatically generated when a new recommendation is created.
        /// Used for database relationships and record identification.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to the Vehicle this recommendation is for.
        /// This creates a relationship between the recommendation
        /// and the specific vehicle in the database.
        /// </summary>
        public int VehicleId { get; set; }

        /// <summary>
        /// The vehicle component that needs maintenance (e.g., "Brakes", "Oil Filter").
        /// This is used for:
        /// - Component-specific maintenance tracking
        /// - Maintenance categorization
        /// - Service planning
        /// - Parts ordering
        /// </summary>
        public required string Component { get; set; }

        /// <summary>
        /// Detailed description of the recommended maintenance.
        /// This should include:
        /// - Required maintenance work
        /// - Expected outcomes
        /// - Potential risks if not performed
        /// - Special considerations
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// Date when the maintenance should be performed.
        /// This is calculated based on:
        /// - Vehicle age
        /// - Last maintenance date
        /// - Manufacturer recommendations
        /// - Current vehicle condition
        /// </summary>
        public DateTime RecommendedDate { get; set; }

        /// <summary>
        /// Mileage at which the maintenance should be performed.
        /// This is used for:
        /// - Mileage-based maintenance scheduling
        /// - Service interval tracking
        /// - Maintenance planning
        /// </summary>
        public double RecommendedMileage { get; set; }

        /// <summary>
        /// Priority level of the maintenance recommendation.
        /// This determines:
        /// - Maintenance urgency
        /// - Scheduling priority
        /// - Notification frequency
        /// See PriorityLevel enum for available options.
        /// </summary>
        public PriorityLevel Priority { get; set; }

        /// <summary>
        /// Indicates whether the vehicle owner has acknowledged this recommendation.
        /// This is used to:
        /// - Track recommendation status
        /// - Manage notifications
        /// - Generate reports
        /// </summary>
        public bool IsAcknowledged { get; set; }

        /// <summary>
        /// Timestamp when this recommendation was created.
        /// This is automatically set when a new recommendation is created.
        /// Used for:
        /// - Recommendation tracking
        /// - Age-based prioritization
        /// - System maintenance
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Estimated cost of the maintenance task.
        /// This is used for:
        /// - Budgeting and cost management
        /// - Service provider billing
        /// - Financial planning
        /// </summary>
        public decimal EstimatedCost { get; set; }

        /// <summary>
        /// Additional notes or comments related to the maintenance task.
        /// This is used for:
        /// - Documentation and record keeping
        /// - Communication with service providers
        /// - Additional context for maintenance tasks
        /// </summary>
        public required string Notes { get; set; }

        /// <summary>
        /// Indicates whether the maintenance task has been completed.
        /// This is used for:
        /// - Tracking completion status
        /// - Managing task history
        /// - Generating reports
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Navigation property to the Vehicle this recommendation is for.
        /// This allows easy access to the vehicle's information
        /// when working with recommendation data.
        /// </summary>
        public required Vehicle Vehicle { get; set; }
    }

    /// <summary>
    /// Enumeration defining the priority levels for maintenance recommendations.
    /// This determines how urgently a maintenance task needs to be performed.
    /// 
    /// Usage:
    /// PriorityLevel priority = PriorityLevel.High;
    /// </summary>
    public enum PriorityLevel
    {
        /// <summary>
        /// Non-urgent maintenance that can be scheduled at convenience.
        /// Examples:
        /// - Regular cleaning
        /// - Minor cosmetic repairs
        /// - Optional upgrades
        /// </summary>
        Low,

        /// <summary>
        /// Important maintenance that should be planned soon.
        /// Examples:
        /// - Regular service intervals
        /// - Preventive maintenance
        /// - Minor repairs
        /// </summary>
        Medium,

        /// <summary>
        /// Urgent maintenance that should be scheduled immediately.
        /// Examples:
        /// - Brake system maintenance
        /// - Engine performance issues
        /// - Safety system checks
        /// </summary>
        High,

        /// <summary>
        /// Critical maintenance that requires immediate attention.
        /// Examples:
        /// - Safety-critical repairs
        /// - Major system failures
        /// - Emergency maintenance
        /// </summary>
        Critical
    }
} 