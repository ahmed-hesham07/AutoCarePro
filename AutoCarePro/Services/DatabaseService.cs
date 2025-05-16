using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AutoCarePro.Models;
using AutoCarePro.Data;

namespace AutoCarePro.Services
{
    public class DatabaseService
    {
        private readonly AutoCareProContext _context;

        public DatabaseService()
        {
            _context = new AutoCareProContext();
        }

        // User Operations
        public User GetUserById(int id)
        {
            return _context.Users
                .Include(u => u.Vehicles)
                .FirstOrDefault(u => u.Id == id);
        }

        public User GetUserByUsername(string username)
        {
            return _context.Users
                .Include(u => u.Vehicles)
                .FirstOrDefault(u => u.Username == username);
        }

        // Vehicle Operations
        public List<Vehicle> GetVehiclesByUserId(int userId)
        {
            return _context.Vehicles
                .Include(v => v.MaintenanceHistory)
                .Include(v => v.Recommendations)
                .Where(v => v.UserId == userId)
                .ToList();
        }

        public Vehicle GetVehicleById(int id)
        {
            return _context.Vehicles
                .Include(v => v.MaintenanceHistory)
                .Include(v => v.Recommendations)
                .FirstOrDefault(v => v.Id == id);
        }

        // Maintenance Operations
        public List<MaintenanceRecord> GetMaintenanceHistory(int vehicleId)
        {
            return _context.MaintenanceRecords
                .Where(m => m.VehicleId == vehicleId)
                .OrderByDescending(m => m.MaintenanceDate)
                .ToList();
        }

        public List<MaintenanceRecommendation> GetRecommendations(int vehicleId)
        {
            return _context.MaintenanceRecommendations
                .Where(r => r.VehicleId == vehicleId)
                .OrderByDescending(r => r.Priority)
                .ToList();
        }

        // Save Changes
        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public void AddMaintenanceRecord(MaintenanceRecord record)
        {
            _context.MaintenanceRecords.Add(record);
        }

        public void AddVehicle(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
        }

        public void DeleteMaintenanceRecord(int recordId)
        {
            var record = _context.MaintenanceRecords.Find(recordId);
            if (record != null)
            {
                _context.MaintenanceRecords.Remove(record);
            }
        }

        public void DeleteVehicle(int vehicleId)
        {
            var vehicle = _context.Vehicles
                .Include(v => v.MaintenanceHistory)
                .Include(v => v.Recommendations)
                .FirstOrDefault(v => v.Id == vehicleId);

            if (vehicle != null)
            {
                // Delete associated records first
                _context.MaintenanceRecords.RemoveRange(vehicle.MaintenanceHistory);
                _context.MaintenanceRecommendations.RemoveRange(vehicle.Recommendations);
                _context.Vehicles.Remove(vehicle);
            }
        }

        public void AddDiagnosisRecommendation(DiagnosisRecommendation recommendation)
        {
            _context.DiagnosisRecommendations.Add(recommendation);
        }

        public List<DiagnosisRecommendation> GetDiagnosisRecommendations(int maintenanceRecordId)
        {
            return _context.DiagnosisRecommendations
                .Where(dr => dr.MaintenanceRecordId == maintenanceRecordId)
                .ToList();
        }

        public List<DiagnosisRecommendation> GetPendingDiagnosisRecommendations(int vehicleId)
        {
            return _context.DiagnosisRecommendations
                .Where(dr => dr.MaintenanceRecord.VehicleId == vehicleId && !dr.IsCompleted)
                .OrderBy(dr => dr.Priority)
                .ThenBy(dr => dr.RecommendedDate)
                .ToList();
        }

        public void UpdateDiagnosisRecommendation(DiagnosisRecommendation recommendation)
        {
            var existing = _context.DiagnosisRecommendations.Find(recommendation.Id);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(recommendation);
            }
        }

        public void DeleteDiagnosisRecommendation(int recommendationId)
        {
            var recommendation = _context.DiagnosisRecommendations.Find(recommendationId);
            if (recommendation != null)
            {
                _context.DiagnosisRecommendations.Remove(recommendation);
            }
        }
    }
}
