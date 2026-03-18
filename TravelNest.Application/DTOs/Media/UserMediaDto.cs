using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TravelNest.Domain.Enums;

namespace TravelNest.Application.DTOs.Media
{
    public class UserMediaDto
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public MediaType Type { get; set; }
        public string TypeName => Type.ToString();
        public string? Caption { get; set; }
        public string? Location { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public int ViewsCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime CreatedAt { get; set; }

        // User info
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserProfilePic { get; set; }

        // Linked property
        public Guid? PropertyId { get; set; }
        public string? PropertyName { get; set; }

        // Comments
        public List<MediaCommentDto> RecentComments { get; set; } = new();
    }
}
