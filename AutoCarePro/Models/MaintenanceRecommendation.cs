using System;

namespace AutoCarePro.Models
{
    public class MaintenanceRecommendation
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string Component { get; set; }
        public string Description { get; set; }
        public DateTime RecommendedDate { get; set; }
        public double RecommendedMileage { get; set; }
        public PriorityLevel Priority { get; set; }
        public bool IsAcknowledged { get; set; }
        public DateTime CreatedDate { get; set; }
        public Vehicle Vehicle { get; set; }
    }

    public enum PriorityLevel
    {
        Low,
        Medium,
        High,
        Critical
    }
} 