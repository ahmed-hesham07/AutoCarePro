using System;
using System.Collections.Generic;
using System.Linq;
using AutoCarePro.Models;

namespace AutoCarePro.Services
{
    public class RecommendationEngine
    {
        private readonly DatabaseService _dbService;
        private const int OIL_CHANGE_MILEAGE = 5000;
        private const int TIRE_ROTATION_MILEAGE = 7500;
        private const int BRAKE_SERVICE_MILEAGE = 15000;
        private const int OIL_CHANGE_MONTHS = 6;
        private const int TIRE_ROTATION_MONTHS = 6;
        private const int BRAKE_SERVICE_MONTHS = 12;

        public RecommendationEngine(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public List<MaintenanceRecommendation> GenerateRecommendations(int vehicleId)
        {
            var vehicle = _dbService.GetVehicleById(vehicleId);
            if (vehicle == null) return new List<MaintenanceRecommendation>();

            var recommendations = new List<MaintenanceRecommendation>();

            if (vehicle is Car car)
            {
                // Check Oil Change
                CheckOilChange(car, recommendations);

                // Check Tire Rotation
                CheckTireRotation(car, recommendations);

                // Check Brake Service
                CheckBrakeService(car, recommendations);
            }

            return recommendations;
        }

        private void CheckOilChange(Car car, List<MaintenanceRecommendation> recommendations)
        {
            var mileageSinceLastChange = car.CurrentMileage - car.OilChangeMileage;
            var monthsSinceLastChange = (DateTime.Now - car.LastOilChange).TotalDays / 30;

            if (mileageSinceLastChange >= OIL_CHANGE_MILEAGE || monthsSinceLastChange >= OIL_CHANGE_MONTHS)
            {
                recommendations.Add(new MaintenanceRecommendation
                {
                    VehicleId = car.Id,
                    Component = "Engine Oil",
                    Description = "Oil change required",
                    RecommendedDate = DateTime.Now,
                    RecommendedMileage = car.CurrentMileage,
                    Priority = DeterminePriority(mileageSinceLastChange, OIL_CHANGE_MILEAGE),
                    CreatedDate = DateTime.Now
                });
            }
        }

        private void CheckTireRotation(Car car, List<MaintenanceRecommendation> recommendations)
        {
            var mileageSinceLastRotation = car.CurrentMileage - car.TireRotationMileage;
            var monthsSinceLastRotation = (DateTime.Now - car.LastTireRotation).TotalDays / 30;

            if (mileageSinceLastRotation >= TIRE_ROTATION_MILEAGE || monthsSinceLastRotation >= TIRE_ROTATION_MONTHS)
            {
                recommendations.Add(new MaintenanceRecommendation
                {
                    VehicleId = car.Id,
                    Component = "Tires",
                    Description = "Tire rotation required",
                    RecommendedDate = DateTime.Now,
                    RecommendedMileage = car.CurrentMileage,
                    Priority = DeterminePriority(mileageSinceLastRotation, TIRE_ROTATION_MILEAGE),
                    CreatedDate = DateTime.Now
                });
            }
        }

        private void CheckBrakeService(Car car, List<MaintenanceRecommendation> recommendations)
        {
            var mileageSinceLastService = car.CurrentMileage - car.BrakeServiceMileage;
            var monthsSinceLastService = (DateTime.Now - car.LastBrakeService).TotalDays / 30;

            if (mileageSinceLastService >= BRAKE_SERVICE_MILEAGE || monthsSinceLastService >= BRAKE_SERVICE_MONTHS)
            {
                recommendations.Add(new MaintenanceRecommendation
                {
                    VehicleId = car.Id,
                    Component = "Brakes",
                    Description = "Brake service required",
                    RecommendedDate = DateTime.Now,
                    RecommendedMileage = car.CurrentMileage,
                    Priority = DeterminePriority(mileageSinceLastService, BRAKE_SERVICE_MILEAGE),
                    CreatedDate = DateTime.Now
                });
            }
        }

        private PriorityLevel DeterminePriority(double currentMileage, double recommendedMileage)
        {
            var percentage = (currentMileage / recommendedMileage) * 100;

            if (percentage >= 150) return PriorityLevel.Critical;
            if (percentage >= 125) return PriorityLevel.High;
            if (percentage >= 100) return PriorityLevel.Medium;
            return PriorityLevel.Low;
        }
    }
}
