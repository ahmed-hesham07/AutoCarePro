using System;
using System.Collections.Generic;

namespace AutoCarePro.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public UserType Type { get; set; }
        public List<Vehicle> Vehicles { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastLoginDate { get; set; }

        public User()
        {
            Vehicles = new List<Vehicle>();
            CreatedDate = DateTime.Now;
        }
    }

    public enum UserType
    {
        CarOwner,
        MaintenanceCenter
    }
} 