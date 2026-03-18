
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TravelNest.Application.DTOs.Bookings;
using TravelNest.Application.DTOs.Common;
using TravelNest.Application.Interfaces;
using TravelNest.Domain.Entities;
using TravelNest.Domain.Enums;
using TravelNest.Domain.Interfaces;

namespace TravelNest.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PaymentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PaymentDto>> CreatePaymentAsync(Guid providerId, CreatePaymentDto dto)
    {
        var booking = await _unitOfWork.Bookings.Query()
            .Include(b => b.Property)
            .FirstOrDefaultAsync(b => b.Id == dto.BookingId);

        if (booking == null)
            return ApiResponse<PaymentDto>.FailResponse("Booking not found");

        if (booking.Property.ProviderId != providerId)
            return ApiResponse<PaymentDto>.FailResponse("Access denied");

        if (booking.Status != BookingStatus.Confirmed)
            return ApiResponse<PaymentDto>.FailResponse("Booking must be confirmed first");

        var existingPayment = await _unitOfWork.Payments
            .FirstOrDefaultAsync(p => p.BookingId == dto.BookingId);

        if (existingPayment != null)
            return ApiResponse<PaymentDto>.FailResponse("Payment already exists for this booking");

        var payment = new Payment
        {
            BookingId = dto.BookingId,
            Amount = booking.TotalPrice,
            Currency = booking.Property.Currency,
            Method = dto.Method,
            Status = dto.Method == PaymentMethod.Cash ? PaymentStatus.Pending : PaymentStatus.Pending,
            PaymentLinkUrl = dto.PaymentLinkUrl,
            ProviderNote = dto.ProviderNote,
            TransactionReference = $"PAY-{Guid.NewGuid().ToString()[..8].ToUpper()}"
        };

        await _unitOfWork.Payments.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<PaymentDto>.SuccessResponse(
            _mapper.Map<PaymentDto>(payment), "Payment created");
    }

    public async Task<ApiResponse<PaymentDto>> UpdatePaymentStatusAsync(
        Guid paymentId, Guid userId, UpdatePaymentStatusDto dto)
    {
        var payment = await _unitOfWork.Payments.Query()
            .Include(p => p.Booking).ThenInclude(b => b.Property)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null)
            return ApiResponse<PaymentDto>.FailResponse("Payment not found");

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (payment.Booking.Property.ProviderId != userId && user?.Role != UserRole.SuperAdmin)
            return ApiResponse<PaymentDto>.FailResponse("Access denied");

        payment.Status = dto.Status;
        if (!string.IsNullOrEmpty(dto.TransactionReference))
            payment.TransactionReference = dto.TransactionReference;

        if (dto.Status == PaymentStatus.Paid)
        {
            payment.PaidAt = DateTime.UtcNow;
            payment.Booking.Status = BookingStatus.Completed;
            _unitOfWork.Bookings.Update(payment.Booking);
        }

        _unitOfWork.Payments.Update(payment);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<PaymentDto>.SuccessResponse(
            _mapper.Map<PaymentDto>(payment), "Payment status updated");
    }

    public async Task<ApiResponse<PaymentDto>> GetByBookingAsync(Guid bookingId, Guid userId)
    {
        var payment = await _unitOfWork.Payments.Query()
            .Include(p => p.Booking).ThenInclude(b => b.Property)
            .FirstOrDefaultAsync(p => p.BookingId == bookingId);

        if (payment == null)
            return ApiResponse<PaymentDto>.FailResponse("Payment not found");

        return ApiResponse<PaymentDto>.SuccessResponse(_mapper.Map<PaymentDto>(payment));
    }

    public async Task<ApiResponse<PagedResponse<PaymentDto>>> GetAllPaymentsAsync(PagedRequest request)
    {
        var query = _unitOfWork.Payments.Query()
            .Include(p => p.Booking)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var payments = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return ApiResponse<PagedResponse<PaymentDto>>.SuccessResponse(new PagedResponse<PaymentDto>
        {
            Data = _mapper.Map<IEnumerable<PaymentDto>>(payments),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }
}
