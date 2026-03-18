using System.ComponentModel.DataAnnotations;

namespace TravelNest.Application.DTOs.Media
{
    public class MediaCommentDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime CreatedAt { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserProfilePic { get; set; }
    }
}
