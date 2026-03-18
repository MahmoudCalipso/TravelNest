using System;
using System.Collections.Generic;
using System.Text;

namespace TravelNest.Domain.Entities
{
    public class ContactMessage : BaseEntity
    {
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public string? Reply { get; set; }

        // Foreign Keys
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public Guid? BookingId { get; set; }

        // Navigation
        public User Sender { get; set; } = null!;
        public User Receiver { get; set; } = null!;
        public Booking? Booking { get; set; }
    }
}
