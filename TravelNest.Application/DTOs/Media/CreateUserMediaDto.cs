using System;
using System.Collections.Generic;
using System.Text;
using TravelNest.Domain.Enums;

namespace TravelNest.Application.DTOs.Media
{
    public class CreateUserMediaDto
    {
        public MediaType Type { get; set; }
        public string? Caption { get; set; }
        public string? Location { get; set; }
        public Guid? PropertyId { get; set; }
    }
}
