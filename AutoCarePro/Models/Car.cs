using System;

namespace AutoCarePro.Models
{
    public class Car : Vehicle
    {
        public string EngineType { get; set; }
        public string TransmissionType { get; set; }
        public double FuelEfficiency { get; set; }
        public DateTime LastOilChange { get; set; }
        public double OilChangeMileage { get; set; }
        public DateTime LastTireRotation { get; set; }
        public double TireRotationMileage { get; set; }
        public DateTime LastBrakeService { get; set; }
        public double BrakeServiceMileage { get; set; }
    }
} 