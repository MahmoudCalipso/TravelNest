using System;
using System.Collections.Generic;
using System.Text;
using TravelNest.Domain.Enums;

namespace TravelNest.Domain.Entities
{
    public class PropertyMedia : BaseEntity
    {
        public string Url { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public MediaType Type { get; set; }
        public string? Caption { get; set; }
        public int SortOrder { get; set; } = 0;
        public bool IsCover { get; set; } = false;

        // Foreign Keys
        public Guid PropertyId { get; set; }

        // Navigation
        public Property Property { get; set; } = null!;
    }
}
