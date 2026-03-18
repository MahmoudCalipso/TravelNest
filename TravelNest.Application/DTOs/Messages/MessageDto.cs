using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TravelNest.Application.DTOs.Messages
{
    public class MessageDto
    {
        public Guid Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public string? Reply { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime CreatedAt { get; set; }

        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string? SenderProfilePic { get; set; }

        public Guid ReceiverId { get; set; }
        public string ReceiverName { get; set; } = string.Empty;

        public Guid? BookingId { get; set; }
        public string? BookingReference { get; set; }
    }
}
