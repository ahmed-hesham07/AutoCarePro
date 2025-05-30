using System;
using System.ComponentModel.DataAnnotations;

namespace AutoCarePro.Models
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int ServiceProviderId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public int DurationMinutes { get; set; }

        // Navigation properties
        public virtual User ServiceProvider { get; set; }
    }
} 