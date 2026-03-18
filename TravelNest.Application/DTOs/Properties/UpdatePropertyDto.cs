using System;
using System.Collections.Generic;
using System.Text;
using TravelNest.Domain.Enums;

namespace TravelNest.Application.DTOs.Properties
{
    public class UpdatePropertyDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public PropertyType? Type { get; set; }
        public decimal? PricePerNight { get; set; }
        public string? Currency { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? MaxGuests { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public bool? IsAvailable { get; set; }
        public List<string>? Amenities { get; set; }
    }
}
