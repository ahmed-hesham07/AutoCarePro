using System;
using System.Collections.Generic;
using System.Linq;
using AutoCarePro.Models;

namespace AutoCarePro.Services
{
    /// <summary>
    /// RecommendationEngine class generates maintenance recommendations for vehicles
    /// based on mileage and time intervals. It analyzes vehicle data and suggests
    /// maintenance tasks when they are due.
    /// </summary>
    public class RecommendationEngine
    {
        // Service for database operations
        private readonly DatabaseService _dbService;

        // Constants for maintenance intervals
        private const int OIL_CHANGE_MILEAGE = 5000;      // Miles between oil changes
        private const int TIRE_ROTATION_MILEAGE = 7500;   // Miles between tire rotations
        private const int BRAKE_SERVICE_MILEAGE = 15000;  // Miles between brake services
        private const int OIL_CHANGE_MONTHS = 6;          // Months between oil changes
        private const int TIRE_ROTATION_MONTHS = 6;       // Months between tire rotations
        private const int BRAKE_SERVICE_MONTHS = 12;      // Months between brake services

        /// <summary>
        /// Constructor initializes the recommendation engine with a database service
        /// </summary>
        /// <param name="dbService">The database service to use for data access</param>
        public RecommendationEngine(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        /// <summary>
        /// Generates maintenance recommendations for a specific vehicle
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle to generate recommendations for</param>
        /// <returns>A list of maintenance recommendations</returns>
        public List<MaintenanceRecommendation> GenerateRecommendations(int vehicleId)
        {
            // Get vehicle data from database
            var vehicle = _dbService.GetVehicleById(vehicleId);
            if (vehicle == null) return new List<MaintenanceRecommendation>();

            var recommendations = new List<MaintenanceRecommendation>();

            // Only generate recommendations for cars
            if (vehicle is Car car)
            {
                // Check various maintenance items
                CheckOilChange(car, recommendations);
                CheckTireRotation(car, recommendations);
                CheckBrakeService(car, recommendations);
            }

            return recommendations;
        }

        /// <summary>
        /// Checks if an oil change is needed based on mileage and time
        /// </summary>
        /// <param name="car">The car to check</param>
        /// <param name="recommendations">List to add recommendations to</param>
        private void CheckOilChange(Car car, List<MaintenanceRecommendation> recommendations)
        {
            // Calculate time and mileage since last oil change
            var mileageSinceLastChange = car.CurrentMileage - car.OilChangeMileage;
            var monthsSinceLastChange = (DateTime.Now - car.LastOilChange).TotalDays / 30;

            // Check if oil change is due based on mileage or time
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
                    CreatedDate = DateTime.Now,
                    Notes = "Auto-generated recommendation",
                    Vehicle = car
                });
            }
        }

        /// <summary>
        /// Checks if a tire rotation is needed based on mileage and time
        /// </summary>
        /// <param name="car">The car to check</param>
        /// <param name="recommendations">List to add recommendations to</param>
        private void CheckTireRotation(Car car, List<MaintenanceRecommendation> recommendations)
        {
            // Calculate time and mileage since last tire rotation
            var mileageSinceLastRotation = car.CurrentMileage - car.TireRotationMileage;
            var monthsSinceLastRotation = (DateTime.Now - car.LastTireRotation).TotalDays / 30;

            // Check if tire rotation is due based on mileage or time
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
                    CreatedDate = DateTime.Now,
                    Notes = "Auto-generated recommendation",
                    Vehicle = car
                });
            }
        }

        /// <summary>
        /// Checks if brake service is needed based on mileage and time
        /// </summary>
        /// <param name="car">The car to check</param>
        /// <param name="recommendations">List to add recommendations to</param>
        private void CheckBrakeService(Car car, List<MaintenanceRecommendation> recommendations)
        {
            // Calculate time and mileage since last brake service
            var mileageSinceLastService = car.CurrentMileage - car.BrakeServiceMileage;
            var monthsSinceLastService = (DateTime.Now - car.LastBrakeService).TotalDays / 30;

            // Check if brake service is due based on mileage or time
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
                    CreatedDate = DateTime.Now,
                    Notes = "Auto-generated recommendation",
                    Vehicle = car
                });
            }
        }

        /// <summary>
        /// Determines the priority level of a maintenance recommendation based on how overdue it is
        /// </summary>
        /// <param name="currentMileage">Current mileage since last maintenance</param>
        /// <param name="recommendedMileage">Recommended mileage interval</param>
        /// <returns>Priority level for the recommendation</returns>
        private PriorityLevel DeterminePriority(double currentMileage, double recommendedMileage)
        {
            // Calculate how overdue the maintenance is as a percentage
            var percentage = (currentMileage / recommendedMileage) * 100;

            // Assign priority based on how overdue the maintenance is
            if (percentage >= 150) return PriorityLevel.Critical;    // 50% or more overdue
            if (percentage >= 125) return PriorityLevel.High;        // 25% or more overdue
            if (percentage >= 100) return PriorityLevel.Medium;      // Due or slightly overdue
            return PriorityLevel.Low;                                // Not yet due
        }
    }
}
