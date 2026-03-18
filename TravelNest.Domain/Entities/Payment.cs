using System;
using System.Collections.Generic;
using System.Text;
using TravelNest.Domain.Enums;

namespace TravelNest.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? PaymentLinkUrl { get; set; }
        public string? TransactionReference { get; set; }
        public string? ProviderNote { get; set; }
        public DateTime? PaidAt { get; set; }

        // Foreign Keys
        public Guid BookingId { get; set; }

        // Navigation
        public Booking Booking { get; set; } = null!;
    }
}
