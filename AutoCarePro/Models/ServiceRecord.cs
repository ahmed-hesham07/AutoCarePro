using System;
using System.Collections.Generic;

namespace AutoCarePro.Models
{
    public class ServiceRecord
    {
        public int Id { get; set; }
        public int ServiceRequestId { get; set; }
        public DateTime ServiceDate { get; set; }
        public string ServiceType { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public string Technician { get; set; }
        public string Status { get; set; }
        public List<string> PartsUsed { get; set; }
        public string Notes { get; set; }
        public int Mileage { get; set; }
        public DateTime? CompletionDate { get; set; }
    }
} 