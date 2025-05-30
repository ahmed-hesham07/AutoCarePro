using System;

namespace AutoCarePro.Models
{
    /// <summary>
    /// Car class represents a car in the AutoCarePro system.
    /// This class inherits from Vehicle and adds car-specific properties and maintenance tracking.
    /// 
    /// Key features:
    /// 1. Tracks car-specific maintenance schedules
    /// 2. Monitors engine and transmission details
    /// 3. Records fuel efficiency metrics
    /// 4. Manages routine maintenance dates
    /// 5. Tracks maintenance mileage intervals
    /// 
    /// Usage:
    /// This class is used throughout the application to:
    /// - Track car-specific maintenance
    /// - Monitor routine service intervals
    /// - Calculate maintenance schedules
    /// - Generate car-specific reports
    /// </summary>
    public class Car : Vehicle
    {
        /// <summary>
        /// Gets or sets the engine type of the vehicle.
        /// Used for maintenance scheduling and parts ordering.
        /// </summary>
        public required string EngineType { get; set; }

        /// <summary>
        /// Gets or sets the transmission type of the vehicle.
        /// Used for maintenance scheduling and parts ordering.
        /// </summary>
        public required string TransmissionType { get; set; }

        /// <summary>
        /// Fuel efficiency rating in miles per gallon (MPG) or kilometers per liter.
        /// This is used for:
        /// - Fuel consumption tracking
        /// - Cost calculations
        /// - Environmental impact assessment
        /// - Performance monitoring
        /// </summary>
        public double FuelEfficiency { get; set; }

        /// <summary>
        /// Date when the last oil change was performed.
        /// This is used to:
        /// - Track oil change intervals
        /// - Schedule next oil change
        /// - Monitor maintenance history
        /// - Generate maintenance reminders
        /// </summary>
        public DateTime LastOilChange { get; set; }

        /// <summary>
        /// Mileage at which the last oil change was performed.
        /// This is used for:
        /// - Oil change interval tracking
        /// - Next service calculation
        /// - Maintenance history
        /// - Service recommendations
        /// </summary>
        public double OilChangeMileage { get; set; }

        /// <summary>
        /// Date when the last tire rotation was performed.
        /// This is used to:
        /// - Track tire rotation intervals
        /// - Schedule next rotation
        /// - Monitor tire maintenance
        /// - Generate maintenance reminders
        /// </summary>
        public DateTime LastTireRotation { get; set; }

        /// <summary>
        /// Mileage at which the last tire rotation was performed.
        /// This is used for:
        /// - Tire rotation interval tracking
        /// - Next service calculation
        /// - Maintenance history
        /// - Service recommendations
        /// </summary>
        public double TireRotationMileage { get; set; }

        /// <summary>
        /// Date when the last brake service was performed.
        /// This is used to:
        /// - Track brake service intervals
        /// - Schedule next service
        /// - Monitor brake maintenance
        /// - Generate maintenance reminders
        /// </summary>
        public DateTime LastBrakeService { get; set; }

        /// <summary>
        /// Mileage at which the last brake service was performed.
        /// This is used for:
        /// - Brake service interval tracking
        /// - Next service calculation
        /// - Maintenance history
        /// - Service recommendations
        /// </summary>
        public double BrakeServiceMileage { get; set; }

        /// <summary>
        /// Gets or sets the color of the vehicle.
        /// Used for vehicle identification and documentation.
        /// </summary>
        public required string Color { get; set; }

        /// <summary>
        /// The number of doors on the car.
        /// This is used for:
        /// - Vehicle specification
        /// - Parts ordering
        /// - Documentation
        /// </summary>
        public int NumberOfDoors { get; set; }

        /// <summary>
        /// Gets or sets the body style of the vehicle.
        /// Used for vehicle classification and service recommendations.
        /// </summary>
        public required string BodyStyle { get; set; }
    }
} 