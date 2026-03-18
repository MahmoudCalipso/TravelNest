using System;
using System.Collections.Generic;
using System.Text;
using TravelNest.Domain.Enums;

namespace TravelNest.Application.DTOs.Bookings
{
    public class UpdateBookingStatusDto
    {
        public BookingStatus Status { get; set; }
        public string? CancellationReason { get; set; }
    }
}
