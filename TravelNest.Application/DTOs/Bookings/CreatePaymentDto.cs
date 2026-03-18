using System;
using System.Collections.Generic;
using System.Text;
using TravelNest.Domain.Enums;

namespace TravelNest.Application.DTOs.Bookings
{
    public class CreatePaymentDto
    {
        public Guid BookingId { get; set; }
        public PaymentMethod Method { get; set; }
        public string? PaymentLinkUrl { get; set; }
        public string? ProviderNote { get; set; }
    }
}
