using System;
using System.Collections.Generic;
using System.Text;
using TravelNest.Application.DTOs.Bookings;
using TravelNest.Application.DTOs.Common;

namespace TravelNest.Application.Interfaces;

public interface IBookingService
{
    Task<ApiResponse<BookingDto>> CreateAsync(Guid travellerId, CreateBookingDto dto);
    Task<ApiResponse<BookingDto>> GetByIdAsync(Guid bookingId, Guid userId);
    Task<ApiResponse<PagedResponse<BookingDto>>> GetByTravellerAsync(Guid travellerId, PagedRequest request);
    Task<ApiResponse<PagedResponse<BookingDto>>> GetByProviderAsync(Guid providerId, PagedRequest request);
    Task<ApiResponse<PagedResponse<BookingDto>>> GetAllAsync(PagedRequest request);
    Task<ApiResponse<BookingDto>> UpdateStatusAsync(Guid bookingId, Guid userId, UpdateBookingStatusDto dto);
    Task<ApiResponse<BookingDto>> CancelBookingAsync(Guid bookingId, Guid userId, string? reason);
}