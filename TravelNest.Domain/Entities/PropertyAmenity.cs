using System;
using System.Collections.Generic;
using System.Text;

namespace TravelNest.Domain.Entities
{
    public class PropertyAmenity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;   // WiFi, Pool, Parking, etc.
        public string? Icon { get; set; }

        // Foreign Keys
        public Guid PropertyId { get; set; }

        // Navigation
        public Property Property { get; set; } = null!;
    }
}
