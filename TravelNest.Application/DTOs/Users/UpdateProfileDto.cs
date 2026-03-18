using System;
using System.Collections.Generic;
using System.Text;

namespace TravelNest.Application.DTOs.Users
{
    public class UpdateProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Bio { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
    }
}
