using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TravelNest.Application.DTOs.Bookings;
using TravelNest.Application.DTOs.Common;
using TravelNest.Application.Interfaces;
using TravelNest.Domain.Entities;
using TravelNest.Domain.Enums;
using TravelNest.Domain.Interfaces;

namespace TravelNest.Infrastructure.Services;

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BookingService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<BookingDto>> CreateAsync(Guid travellerId, CreateBookingDto dto)
    {
        var property = await _unitOfWork.Properties.Query()
            .Include(p => p.Provider)
            .FirstOrDefaultAsync(p => p.Id == dto.PropertyId);

        if (property == null)
            return ApiResponse<BookingDto>.FailResponse("Property not found");

        if (!property.IsApproved || !property.IsAvailable)
            return ApiResponse<BookingDto>.FailResponse("Property is not available");

        if (dto.NumberOfGuests > property.MaxGuests)
            return ApiResponse<BookingDto>.FailResponse($"Max guests allowed: {property.MaxGuests}");

        // Check for overlapping bookings
        var hasConflict = await _unitOfWork.Bookings.AnyAsync(b =>
            b.PropertyId == dto.PropertyId &&
            b.Status != BookingStatus.Cancelled &&
            b.Status != BookingStatus.Rejected &&
            b.CheckInDate < dto.CheckOutDate &&
            b.CheckOutDate > dto.CheckInDate);

        if (hasConflict)
            return ApiResponse<BookingDto>.FailResponse("Property is already booked for these dates");

        var totalNights = (dto.CheckOutDate - dto.CheckInDate).Days;

        var booking = new Booking
        {
            TravellerId = travellerId,
            PropertyId = dto.PropertyId,
            CheckInDate = dto.CheckInDate,
            CheckOutDate = dto.CheckOutDate,
            NumberOfGuests = dto.NumberOfGuests,
            TotalNights = totalNights,
            PricePerNight = property.PricePerNight,
            TotalPrice = property.PricePerNight * totalNights,
            Status = BookingStatus.Pending,
            SpecialRequests = dto.SpecialRequests,
            BookingReference = GenerateBookingReference()
        };

        await _unitOfWork.Bookings.AddAsync(booking);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<BookingDto>.SuccessResponse(
            await GetBookingDtoAsync(booking.Id),
            "Booking created. Awaiting provider confirmation.");
    }

    public async Task<ApiResponse<BookingDto>> GetByIdAsync(Guid bookingId, Guid userId)
    {
        var dto = await GetBookingDtoAsync(bookingId);
        if (dto == null)
            return ApiResponse<BookingDto>.FailResponse("Booking not found");

        // Check access
        if (dto.TravellerId != userId && dto.ProviderId != userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user?.Role != UserRole.SuperAdmin)
                return ApiResponse<BookingDto>.FailResponse("Access denied");
        }

        return ApiResponse<BookingDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<PagedResponse<BookingDto>>> GetByTravellerAsync(
        Guid travellerId, PagedRequest request)
    {
        return await GetBookingsPagedAsync(
            b => b.TravellerId == travellerId, request);
    }

    public async Task<ApiResponse<PagedResponse<BookingDto>>> GetByProviderAsync(
        Guid providerId, PagedRequest request)
    {
        return await GetBookingsPagedAsync(
            b => b.Property.ProviderId == providerId, request);
    }

    public async Task<ApiResponse<PagedResponse<BookingDto>>> GetAllAsync(PagedRequest request)
    {
        return await GetBookingsPagedAsync(null, request);
    }

    public async Task<ApiResponse<BookingDto>> UpdateStatusAsync(
        Guid bookingId, Guid userId, UpdateBookingStatusDto dto)
    {
        var booking = await _unitOfWork.Bookings.Query()
            .Include(b => b.Property)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            return ApiResponse<BookingDto>.FailResponse("Booking not found");

        // Only provider or admin can update status
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (booking.Property.ProviderId != userId && user?.Role != UserRole.SuperAdmin)
            return ApiResponse<BookingDto>.FailResponse("Access denied");

        booking.Status = dto.Status;
        if (dto.Status == BookingStatus.Cancelled)
            booking.CancellationReason = dto.CancellationReason;

        _unitOfWork.Bookings.Update(booking);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<BookingDto>.SuccessResponse(
            await GetBookingDtoAsync(bookingId), "Booking status updated");
    }

    public async Task<ApiResponse<BookingDto>> CancelBookingAsync(
        Guid bookingId, Guid userId, string? reason)
    {
        var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
        if (booking == null)
            return ApiResponse<BookingDto>.FailResponse("Booking not found");

        if (booking.TravellerId != userId)
            return ApiResponse<BookingDto>.FailResponse("Access denied");

        if (booking.Status == BookingStatus.Completed)
            return ApiResponse<BookingDto>.FailResponse("Cannot cancel a completed booking");

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationReason = reason;
        _unitOfWork.Bookings.Update(booking);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<BookingDto>.SuccessResponse(
            await GetBookingDtoAsync(bookingId), "Booking cancelled");
    }

    private async Task<BookingDto?> GetBookingDtoAsync(Guid bookingId)
    {
        var booking = await _unitOfWork.Bookings.Query()
            .Include(b => b.Traveller)
            .Include(b => b.Property).ThenInclude(p => p.Provider)
            .Include(b => b.Property).ThenInclude(p => p.Media)
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        return booking == null ? null : _mapper.Map<BookingDto>(booking);
    }

    private async Task<ApiResponse<PagedResponse<BookingDto>>> GetBookingsPagedAsync(
        System.Linq.Expressions.Expression<Func<Booking, bool>>? filter, PagedRequest request)
    {
        var query = _unitOfWork.Bookings.Query()
            .Include(b => b.Traveller)
            .Include(b => b.Property).ThenInclude(p => p.Provider)
            .Include(b => b.Property).ThenInclude(p => p.Media)
            .Include(b => b.Payment)
            .AsQueryable();

        if (filter != null)
            query = query.Where(filter);

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(b =>
                b.BookingReference.ToLower().Contains(term) ||
                b.Property.Name.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();
        var bookings = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return ApiResponse<PagedResponse<BookingDto>>.SuccessResponse(new PagedResponse<BookingDto>
        {
            Data = _mapper.Map<IEnumerable<BookingDto>>(bookings),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }

    private static string GenerateBookingReference()
        => $"TN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
}
