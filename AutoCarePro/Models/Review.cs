using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoCarePro.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ServiceProviderId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("ServiceProviderId")]
        public User ServiceProvider { get; set; }

        [ForeignKey("CustomerId")]
        public User Customer { get; set; }

        // Computed property for customer name
        [NotMapped]
        public string CustomerName => Customer?.FullName ?? "Unknown Customer";
    }
} 