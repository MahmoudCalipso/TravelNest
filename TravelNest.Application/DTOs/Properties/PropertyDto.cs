using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TravelNest.Domain.Enums;

namespace TravelNest.Application.DTOs.Properties
{
    public class PropertyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PropertyType Type { get; set; }
        public string TypeName => Type.ToString();
        public decimal PricePerNight { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int MaxGuests { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsApproved { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime CreatedAt { get; set; }

        // Provider info
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string? ProviderPhone { get; set; }
        public string? ProviderProfilePic { get; set; }

        // Media & Amenities
        public List<PropertyMediaDto> Media { get; set; } = new();
        public List<string> Amenities { get; set; } = new();
        public bool IsFavorited { get; set; }
    }
}
