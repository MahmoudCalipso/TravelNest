using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TravelNest.Domain.Enums;

namespace TravelNest.Application.DTOs.Bookings
{
    public class BookingDto
    {
        public Guid Id { get; set; }
        public string BookingReference { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime CheckInDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public int TotalNights { get; set; }
        public decimal PricePerNight { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public string? SpecialRequests { get; set; }
        public string? CancellationReason { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime CreatedAt { get; set; }

        // Property info
        public Guid PropertyId { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public string? PropertyCoverImage { get; set; }
        public string PropertyCity { get; set; } = string.Empty;
        public string PropertyCountry { get; set; } = string.Empty;

        // Traveller info
        public Guid TravellerId { get; set; }
        public string TravellerName { get; set; } = string.Empty;
        public string? TravellerEmail { get; set; }
        public string? TravellerPhone { get; set; }

        // Provider info
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;

        // Payment info
        public PaymentDto? Payment { get; set; }
    }
}
