using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TravelNest.Domain.Enums;

namespace TravelNest.Domain.Entities
{
    public class Booking : BaseEntity
    {
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime CheckInDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public int TotalNights { get; set; }
        public decimal PricePerNight { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public string? SpecialRequests { get; set; }
        public string? CancellationReason { get; set; }
        public string BookingReference { get; set; } = string.Empty;

        // Foreign Keys
        public Guid TravellerId { get; set; }
        public Guid PropertyId { get; set; }

        // Navigation
        public User Traveller { get; set; } = null!;
        public Property Property { get; set; } = null!;
        public Payment? Payment { get; set; }
    }
}
