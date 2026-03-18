using System;
using System.Collections.Generic;
using System.Text;

namespace TravelNest.Domain.Entities
{
    public class Favorite : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid PropertyId { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public Property Property { get; set; } = null!;
    }
}
