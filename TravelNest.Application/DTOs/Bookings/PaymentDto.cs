using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TravelNest.Domain.Enums;

namespace TravelNest.Application.DTOs.Bookings
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public PaymentMethod Method { get; set; }
        public string MethodName => Method.ToString();
        public PaymentStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public string? PaymentLinkUrl { get; set; }
        public string? TransactionReference { get; set; }
        public string? ProviderNote { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? PaidAt { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime CreatedAt { get; set; }
    }
}
