using System; // Import the System namespace for basic types like DateTime

namespace AutoCarePro.Models
{
    // This class represents a record of maintenance performed on a vehicle
    public class MaintenanceRecord
    {
        public int Id { get; set; } // Unique identifier for the maintenance record
        public int VehicleId { get; set; } // The ID of the vehicle this record belongs to
        public int? DiagnosedByUserId { get; set; }  // (Optional) ID of the maintenance center user who diagnosed the vehicle
        public DateTime MaintenanceDate { get; set; } // The date when the maintenance was performed
        public string MaintenanceType { get; set; } // The type of maintenance (e.g., Oil Change)
        public string Description { get; set; } // Description of the maintenance work
        public decimal MileageAtMaintenance { get; set; } // The vehicle's mileage at the time of maintenance
        public decimal Cost { get; set; } // The cost of the maintenance
        public string ServiceProvider { get; set; } // The name of the service provider or garage
        public string Notes { get; set; } // Any additional notes about the maintenance
        public bool HasDiagnosisRecommendations { get; set; }  // True if there are diagnosis-based recommendations for this record
        public DateTime CreatedAt { get; set; } // When this record was created
        public DateTime UpdatedAt { get; set; } // When this record was last updated

        // Navigation properties (used by Entity Framework to link related data)
        public Vehicle Vehicle { get; set; } // The vehicle associated with this maintenance record
        public User DiagnosedByUser { get; set; }  // The maintenance center user who diagnosed the vehicle (if any)
    }
} 