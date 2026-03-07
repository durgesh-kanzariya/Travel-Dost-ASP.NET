using System;
using System.ComponentModel.DataAnnotations;

namespace travel_dost_asp.net.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; } = null!;
        
        [System.ComponentModel.DataAnnotations.Schema.Column("password")]
        public string PasswordHash { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Joined properties (not in the users table directly)
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? Role { get; set; }
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? NativeLanguage { get; set; }
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? DefaultCurrency { get; set; }
    }

    public class UserProfile
    {
        public int UserId { get; set; }
        public string? NativeLanguage { get; set; }
        public string? DefaultCurrency { get; set; }
    }

    public class Role
    {
        public int Id { get; set; }
        public string RoleName { get; set; } = null!;
    }

    public class UserRole
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
