using System;
using System.Collections.Generic;
using System.Text;

namespace TravelNest.Domain.Entities
{
    public class MediaLike : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid UserMediaId { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public UserMedia UserMedia { get; set; } = null!;
    }
}
