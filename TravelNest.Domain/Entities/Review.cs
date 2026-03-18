using System;
using System.Collections.Generic;
using System.Text;

namespace TravelNest.Domain.Entities
{
    public class Review : BaseEntity
    {
        public int Rating { get; set; } // 1-5
        public string? Comment { get; set; }

        // Foreign Keys
        public Guid TravellerId { get; set; }
        public Guid PropertyId { get; set; }

        // Navigation
        public User Traveller { get; set; } = null!;
        public Property Property { get; set; } = null!;
    }
}
