using System;
using System.Collections.Generic;
using AutoCarePro.Models;

namespace AutoCarePro.Services
{
    /// <summary>
    /// RoleBasedAccessControl class implements role-based access control (RBAC) for the application.
    /// It defines permissions for different user types and provides methods to check access rights
    /// for various resources like vehicles, maintenance records, and user profiles.
    /// </summary>
    public class RoleBasedAccessControl
    {
        /// <summary>
        /// Dictionary mapping user types to their allowed permissions.
        /// Each user type has a set of string permissions that define what actions they can perform.
        /// </summary>
        private static readonly Dictionary<UserType, HashSet<string>> _permissions = new Dictionary<UserType, HashSet<string>>
        {
            {
                UserType.CarOwner, new HashSet<string>
                {
                    // Permissions for car owners
                    "ViewOwnVehicles",              // Can view their own vehicles
                    "AddOwnVehicle",                // Can add vehicles to their account
                    "EditOwnVehicle",               // Can edit their own vehicles
                    "DeleteOwnVehicle",             // Can delete their own vehicles
                    "ViewOwnMaintenanceRecords",    // Can view maintenance records for their vehicles
                    "AddOwnMaintenanceRecord",      // Can add maintenance records for their vehicles
                    "EditOwnMaintenanceRecord",     // Can edit maintenance records for their vehicles
                    "DeleteOwnMaintenanceRecord",   // Can delete maintenance records for their vehicles
                    "ViewOwnProfile",               // Can view their own profile
                    "EditOwnProfile",               // Can edit their own profile
                    "ChangeOwnPassword"             // Can change their own password
                }
            },
            {
                UserType.MaintenanceCenter, new HashSet<string>
                {
                    // Permissions for maintenance centers
                    "ViewAllVehicles",              // Can view all vehicles
                    "AddVehicle",                   // Can add vehicles
                    "EditVehicle",                  // Can edit vehicles
                    "DeleteVehicle",                // Can delete vehicles
                    "ViewAllMaintenanceRecords",    // Can view all maintenance records
                    "AddMaintenanceRecord",         // Can add maintenance records
                    "EditMaintenanceRecord",        // Can edit maintenance records
                    "DeleteMaintenanceRecord",      // Can delete maintenance records
                    "AddMaintenanceRecommendation", // Can add maintenance recommendations
                    "EditMaintenanceRecommendation",// Can edit maintenance recommendations
                    "DeleteMaintenanceRecommendation",// Can delete maintenance recommendations
                    "AddDiagnosisRecommendation",   // Can add diagnosis recommendations
                    "EditDiagnosisRecommendation",  // Can edit diagnosis recommendations
                    "DeleteDiagnosisRecommendation",// Can delete diagnosis recommendations
                    "ViewOwnProfile",               // Can view their own profile
                    "EditOwnProfile",               // Can edit their own profile
                    "ChangeOwnPassword"             // Can change their own password
                }
            },
            {
                UserType.Admin, new HashSet<string>
                {
                    // Permissions for administrators
                    "ViewAllUsers",                 // Can view all users
                    "AddUser",                      // Can add users
                    "EditUser",                     // Can edit users
                    "DeleteUser",                   // Can delete users
                    "ViewAllVehicles",              // Can view all vehicles
                    "AddVehicle",                   // Can add vehicles
                    "EditVehicle",                  // Can edit vehicles
                    "DeleteVehicle",                // Can delete vehicles
                    "ViewAllMaintenanceRecords",    // Can view all maintenance records
                    "AddMaintenanceRecord",         // Can add maintenance records
                    "EditMaintenanceRecord",        // Can edit maintenance records
                    "DeleteMaintenanceRecord",      // Can delete maintenance records
                    "AddMaintenanceRecommendation", // Can add maintenance recommendations
                    "EditMaintenanceRecommendation",// Can edit maintenance recommendations
                    "DeleteMaintenanceRecommendation",// Can delete maintenance recommendations
                    "AddDiagnosisRecommendation",   // Can add diagnosis recommendations
                    "EditDiagnosisRecommendation",  // Can edit diagnosis recommendations
                    "DeleteDiagnosisRecommendation",// Can delete diagnosis recommendations
                    "ViewOwnProfile",               // Can view their own profile
                    "EditOwnProfile",               // Can edit their own profile
                    "ChangeOwnPassword",            // Can change their own password
                    "ManageSystemSettings"          // Can manage system settings
                }
            }
        };

        /// <summary>
        /// Checks if a user has a specific permission
        /// </summary>
        /// <param name="user">The user to check permissions for</param>
        /// <param name="permission">The permission to check</param>
        /// <returns>True if the user has the permission, false otherwise</returns>
        public static bool HasPermission(User user, string permission)
        {
            if (user == null || string.IsNullOrWhiteSpace(permission))
                return false;

            return _permissions.ContainsKey(user.Type) &&
                   _permissions[user.Type].Contains(permission);
        }

        /// <summary>
        /// Checks if a user can access a specific vehicle
        /// </summary>
        /// <param name="user">The user attempting to access the vehicle</param>
        /// <param name="vehicle">The vehicle being accessed</param>
        /// <returns>True if the user can access the vehicle, false otherwise</returns>
        public static bool CanAccessVehicle(User user, Vehicle vehicle)
        {
            if (user == null || vehicle == null)
                return false;

            // Admins and maintenance centers can access all vehicles
            if (user.Type == UserType.Admin || user.Type == UserType.MaintenanceCenter)
                return true;

            // Car owners can only access their own vehicles
            return user.Type == UserType.CarOwner && vehicle.UserId == user.Id;
        }

        /// <summary>
        /// Checks if a user can access a specific maintenance record
        /// </summary>
        /// <param name="user">The user attempting to access the record</param>
        /// <param name="record">The maintenance record being accessed</param>
        /// <returns>True if the user can access the record, false otherwise</returns>
        public static bool CanAccessMaintenanceRecord(User user, MaintenanceRecord record)
        {
            if (user == null || record == null)
                return false;

            // Admins and maintenance centers can access all records
            if (user.Type == UserType.Admin || user.Type == UserType.MaintenanceCenter)
                return true;

            // Car owners can only access records for their own vehicles
            return user.Type == UserType.CarOwner && record.Vehicle.UserId == user.Id;
        }

        /// <summary>
        /// Checks if a user can access a specific maintenance recommendation
        /// </summary>
        /// <param name="user">The user attempting to access the recommendation</param>
        /// <param name="recommendation">The maintenance recommendation being accessed</param>
        /// <returns>True if the user can access the recommendation, false otherwise</returns>
        public static bool CanAccessMaintenanceRecommendation(User user, MaintenanceRecommendation recommendation)
        {
            if (user == null || recommendation == null)
                return false;

            // Admins and maintenance centers can access all recommendations
            if (user.Type == UserType.Admin || user.Type == UserType.MaintenanceCenter)
                return true;

            // Car owners can only access recommendations for their own vehicles
            return user.Type == UserType.CarOwner && recommendation.Vehicle.UserId == user.Id;
        }

        /// <summary>
        /// Checks if a user can access a specific diagnosis recommendation
        /// </summary>
        /// <param name="user">The user attempting to access the recommendation</param>
        /// <param name="recommendation">The diagnosis recommendation being accessed</param>
        /// <returns>True if the user can access the recommendation, false otherwise</returns>
        public static bool CanAccessDiagnosisRecommendation(User user, DiagnosisRecommendation recommendation)
        {
            if (user == null || recommendation == null)
                return false;

            // Admins and maintenance centers can access all recommendations
            if (user.Type == UserType.Admin || user.Type == UserType.MaintenanceCenter)
                return true;

            // Car owners can only access recommendations for their own vehicles
            return user.Type == UserType.CarOwner && 
                   recommendation.MaintenanceRecord.Vehicle.UserId == user.Id;
        }

        /// <summary>
        /// Checks if a user can access another user's profile
        /// </summary>
        /// <param name="currentUser">The user attempting to access the profile</param>
        /// <param name="targetUser">The user whose profile is being accessed</param>
        /// <returns>True if the current user can access the target user's profile, false otherwise</returns>
        public static bool CanAccessUser(User currentUser, User targetUser)
        {
            if (currentUser == null || targetUser == null)
                return false;

            // Users can always access their own profile
            if (currentUser.Id == targetUser.Id)
                return true;

            // Only admins can access other users' profiles
            return currentUser.Type == UserType.Admin;
        }
    }
} 