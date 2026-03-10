using System;
using System.ComponentModel.DataAnnotations;

namespace travel_dost_asp.net.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Category { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class CustomChecklist
    {
        public int Id { get; set; }
        public int? TripId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ChecklistItem
    {
        public int Id { get; set; }
        public int ChecklistId { get; set; }
        public string ItemName { get; set; } = null!;
        public bool IsChecked { get; set; } = false;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class CountryGuide
    {
        public int Id { get; set; }
        public string CountryName { get; set; } = null!;
        public string? PoliceNumber { get; set; }
        public string? AmbulanceNumber { get; set; }
        public string? FireNumber { get; set; }
        public string? EmbassyNumber { get; set; }
        
        [System.ComponentModel.DataAnnotations.Schema.Column("local_rules")]
        public List<string>? LocalRules { get; set; }
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
