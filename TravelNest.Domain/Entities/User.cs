using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TravelNest.Domain.Enums;

namespace TravelNest.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Bio { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; } = true;
        public string? RefreshToken { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Navigation Properties
        public ICollection<Property> Properties { get; set; } = new List<Property>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<UserMedia> MediaPosts { get; set; } = new List<UserMedia>();
        public ICollection<MediaLike> MediaLikes { get; set; } = new List<MediaLike>();
        public ICollection<MediaComment> MediaComments { get; set; } = new List<MediaComment>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}
