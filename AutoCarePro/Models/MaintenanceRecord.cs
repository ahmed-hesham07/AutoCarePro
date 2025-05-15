using System;

namespace AutoCarePro.Models
{
    public class MaintenanceRecord
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public DateTime MaintenanceDate { get; set; }
        public double MileageAtMaintenance { get; set; }
        public string MaintenanceType { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public string ServiceProvider { get; set; }
        public string Notes { get; set; }
        public bool IsCompleted { get; set; }
    }
} 