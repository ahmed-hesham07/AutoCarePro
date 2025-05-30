using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoCarePro.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public int MaintenanceCenterId { get; set; }

        [Required]
        public int ServiceProviderId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("VehicleId")]
        public Vehicle Vehicle { get; set; }

        [ForeignKey("MaintenanceCenterId")]
        public User MaintenanceCenter { get; set; }

        [ForeignKey("ServiceProviderId")]
        public User ServiceProvider { get; set; }

        // Computed properties for display
        public string CustomerName => Vehicle?.User?.FullName ?? "Unknown";
        public string VehicleInfo => Vehicle != null ? $"{Vehicle.Make} {Vehicle.Model} ({Vehicle.Year})" : "Unknown";
        public string ServiceType { get; set; }
    }
} 