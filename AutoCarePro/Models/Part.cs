using System;

namespace AutoCarePro.Models
{
    public class Part
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PartNumber { get; set; }
        public string Manufacturer { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int QuantityInStock { get; set; }
        public string Category { get; set; }
        public string Compatibility { get; set; }
        public DateTime LastRestocked { get; set; }
        public int MinimumStockLevel { get; set; }
    }
} 