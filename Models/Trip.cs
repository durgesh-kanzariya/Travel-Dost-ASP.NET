using System;
using System.ComponentModel.DataAnnotations;

namespace travel_dost_asp.net.Models
{
    public class Trip
    {
        [Key]
        [System.ComponentModel.DataAnnotations.Schema.Column("id")]
        public int TripId { get; set; }
        
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Budget { get; set; }
        public string Currency { get; set; } = "USD";
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string Status { get; set; } = "Planned";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? Destination { get; set; }
    }

    public class Destination
    {
        public int Id { get; set; }
        public string LocationName { get; set; } = null!;
        public string CountryName { get; set; } = "Unknown";
        public string LocationType { get; set; } = "city";
    }

    public class TripDestination
    {
        public int TripId { get; set; }
        public int DestinationId { get; set; }
        public int VisitOrder { get; set; }
    }
}
