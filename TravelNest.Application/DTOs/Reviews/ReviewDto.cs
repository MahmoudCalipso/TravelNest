using System;
using System.Collections.Generic;
using System.Text;

namespace TravelNest.Application.DTOs.Reviews
{
    public class ReviewDto
    {
        public Guid Id { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid TravellerId { get; set; }
        public string TravellerName { get; set; } = string.Empty;
        public string? TravellerProfilePic { get; set; }
    }
}
