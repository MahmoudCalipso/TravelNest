using System;
using System.Collections.Generic;
using System.Text;
using TravelNest.Domain.Enums;

namespace TravelNest.Domain.Entities
{
    public class UserMedia : BaseEntity
    {
        public string Url { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public MediaType Type { get; set; }
        public string? Caption { get; set; }
        public string? Location { get; set; }
        public int LikesCount { get; set; } = 0;
        public int CommentsCount { get; set; } = 0;
        public int ViewsCount { get; set; } = 0;

        // Foreign Keys
        public Guid UserId { get; set; }
        public Guid? PropertyId { get; set; } // optional tag to a property

        // Navigation
        public User User { get; set; } = null!;
        public Property? Property { get; set; }
        public ICollection<MediaLike> Likes { get; set; } = new List<MediaLike>();
        public ICollection<MediaComment> Comments { get; set; } = new List<MediaComment>();
    }
}
