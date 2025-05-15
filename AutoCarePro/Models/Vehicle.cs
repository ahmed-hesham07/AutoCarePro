using System;
using System.Collections.Generic;

namespace AutoCarePro.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string VIN { get; set; }
        public double CurrentMileage { get; set; }
        public DateTime LastMaintenanceDate { get; set; }
        public List<MaintenanceRecord> MaintenanceHistory { get; set; }
        public List<MaintenanceRecommendation> Recommendations { get; set; }

        public Vehicle()
        {
            MaintenanceHistory = new List<MaintenanceRecord>();
            Recommendations = new List<MaintenanceRecommendation>();
        }
    }
} 