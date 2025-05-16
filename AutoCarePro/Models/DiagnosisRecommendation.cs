using System; // Import the System namespace for basic types like DateTime

namespace AutoCarePro.Models
{
    // This class represents a recommendation made during a vehicle diagnosis
    public class DiagnosisRecommendation
    {
        public int Id { get; set; } // Unique identifier for the diagnosis recommendation
        public int MaintenanceRecordId { get; set; }  // The ID of the maintenance record this recommendation is linked to
        public int DiagnosedByUserId { get; set; }    // The ID of the maintenance center user who made the diagnosis
        public string Component { get; set; }         // The component of the vehicle that needs attention
        public string Description { get; set; }       // Detailed description of the issue or recommendation
        public string Priority { get; set; }          // Priority level: Critical, High, Medium, or Low
        public decimal EstimatedCost { get; set; }    // Estimated cost to fix the issue
        public DateTime RecommendedDate { get; set; } // The date by which the work should be done
        public bool IsCompleted { get; set; }         // True if the recommendation has been completed
        public DateTime CreatedAt { get; set; } // When this recommendation was created
        public DateTime UpdatedAt { get; set; } // When this recommendation was last updated

        // Navigation properties (used by Entity Framework to link related data)
        public MaintenanceRecord MaintenanceRecord { get; set; } // The maintenance record associated with this recommendation
        public User DiagnosedByUser { get; set; } // The maintenance center user who made the diagnosis
    }
} 