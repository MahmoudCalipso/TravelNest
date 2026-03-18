using System;
using System.Collections.Generic;
using System.Text;
using TravelNest.Application.DTOs.Bookings;
using TravelNest.Application.DTOs.Common;

namespace TravelNest.Application.Interfaces;

public interface IPaymentService
{
    Task<ApiResponse<PaymentDto>> CreatePaymentAsync(Guid providerId, CreatePaymentDto dto);
    Task<ApiResponse<PaymentDto>> UpdatePaymentStatusAsync(Guid paymentId, Guid userId, UpdatePaymentStatusDto dto);
    Task<ApiResponse<PaymentDto>> GetByBookingAsync(Guid bookingId, Guid userId);
    Task<ApiResponse<PagedResponse<PaymentDto>>> GetAllPaymentsAsync(PagedRequest request);
}