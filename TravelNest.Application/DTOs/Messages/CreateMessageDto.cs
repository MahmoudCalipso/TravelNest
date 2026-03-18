using System;
using System.Collections.Generic;
using System.Text;

namespace TravelNest.Application.DTOs.Messages
{
    public class CreateMessageDto
    {
        public Guid ReceiverId { get; set; }
        public Guid? BookingId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
