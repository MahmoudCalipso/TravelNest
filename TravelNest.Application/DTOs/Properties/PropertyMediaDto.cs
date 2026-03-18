using System;
using System.Collections.Generic;
using System.Text;
using TravelNest.Domain.Enums;

namespace TravelNest.Application.DTOs.Properties
{
    public class PropertyMediaDto
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public MediaType Type { get; set; }
        public string? Caption { get; set; }
        public int SortOrder { get; set; }
        public bool IsCover { get; set; }
    }
}
