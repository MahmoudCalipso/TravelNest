using System;
using System.Collections.Generic;
using System.Text;
using TravelNest.Domain.Enums;

namespace TravelNest.Application.DTOs.Bookings
{
    public class UpdatePaymentStatusDto
    {
        public PaymentStatus Status { get; set; }
        public string? TransactionReference { get; set; }
    }
}
