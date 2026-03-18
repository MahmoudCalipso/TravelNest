using System;
using System.Collections.Generic;
using System.Text;

namespace TravelNest.Application.DTOs.Reviews
{
    public class CreateReviewDto
    {
        public Guid PropertyId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
