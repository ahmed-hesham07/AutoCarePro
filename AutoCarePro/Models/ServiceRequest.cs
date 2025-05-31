using System;

namespace AutoCarePro.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int CustomerId { get; set; }
        public string Description { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; }
        public decimal EstimatedCost { get; set; }
        public string Priority { get; set; }
        public string Notes { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public string AssignedTechnician { get; set; }
    }
} 