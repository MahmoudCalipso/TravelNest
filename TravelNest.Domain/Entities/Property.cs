using System;
using System.Collections.Generic;
using System.Text;
using TravelNest.Domain.Enums;

namespace TravelNest.Domain.Entities
{
    public class Property : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PropertyType Type { get; set; }
        public decimal PricePerNight { get; set; }
        public string Currency { get; set; } = "USD";
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int MaxGuests { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsApproved { get; set; } = false;
        public double AverageRating { get; set; } = 0;
        public int TotalReviews { get; set; } = 0;

        // Foreign Keys
        public Guid ProviderId { get; set; }

        // Navigation Properties
        public User Provider { get; set; } = null!;
        public ICollection<PropertyMedia> Media { get; set; } = new List<PropertyMedia>();
        public ICollection<PropertyAmenity> Amenities { get; set; } = new List<PropertyAmenity>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<PropertyAvailability> Availabilities { get; set; } = new List<PropertyAvailability>();
    }
}
