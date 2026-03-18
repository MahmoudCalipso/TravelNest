
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TravelNest.Application.DTOs.Common;
using TravelNest.Application.DTOs.Reviews;
using TravelNest.Application.Interfaces;
using TravelNest.Domain.Entities;
using TravelNest.Domain.Enums;
using TravelNest.Domain.Interfaces;

namespace TravelNest.Infrastructure.Services;

public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ReviewService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ReviewDto>> CreateAsync(Guid travellerId, CreateReviewDto dto)
    {
        // Check if user has a completed booking for this property
        var hasCompletedBooking = await _unitOfWork.Bookings.AnyAsync(b =>
            b.TravellerId == travellerId &&
            b.PropertyId == dto.PropertyId &&
            (b.Status == BookingStatus.Completed || b.Status == BookingStatus.Confirmed));

        if (!hasCompletedBooking)
            return ApiResponse<ReviewDto>.FailResponse("You can only review properties you've booked");

        var alreadyReviewed = await _unitOfWork.Reviews.AnyAsync(r =>
            r.TravellerId == travellerId && r.PropertyId == dto.PropertyId);

        if (alreadyReviewed)
            return ApiResponse<ReviewDto>.FailResponse("You already reviewed this property");

        var review = new Review
        {
            TravellerId = travellerId,
            PropertyId = dto.PropertyId,
            Rating = dto.Rating,
            Comment = dto.Comment
        };

        await _unitOfWork.Reviews.AddAsync(review);

        // Update property average rating
        var property = await _unitOfWork.Properties.GetByIdAsync(dto.PropertyId);
        if (property != null)
        {
            var reviews = await _unitOfWork.Reviews
                .FindAsync(r => r.PropertyId == dto.PropertyId);
            var allReviews = reviews.ToList();
            allReviews.Add(review);

            property.AverageRating = allReviews.Average(r => r.Rating);
            property.TotalReviews = allReviews.Count;
            _unitOfWork.Properties.Update(property);
        }

        await _unitOfWork.SaveChangesAsync();

        var createdReview = await _unitOfWork.Reviews.Query()
            .Include(r => r.Traveller)
            .FirstOrDefaultAsync(r => r.Id == review.Id);

        return ApiResponse<ReviewDto>.SuccessResponse(
            _mapper.Map<ReviewDto>(createdReview), "Review submitted");
    }

    public async Task<ApiResponse<PagedResponse<ReviewDto>>> GetByPropertyAsync(
        Guid propertyId, PagedRequest request)
    {
        var query = _unitOfWork.Reviews.Query()
            .Include(r => r.Traveller)
            .Where(r => r.PropertyId == propertyId);

        var totalCount = await query.CountAsync();
        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return ApiResponse<PagedResponse<ReviewDto>>.SuccessResponse(new PagedResponse<ReviewDto>
        {
            Data = _mapper.Map<IEnumerable<ReviewDto>>(reviews),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid reviewId, Guid userId)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
        if (review == null)
            return ApiResponse<bool>.FailResponse("Review not found");

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (review.TravellerId != userId && user?.Role != UserRole.SuperAdmin)
            return ApiResponse<bool>.FailResponse("Access denied");

        review.IsDeleted = true;
        _unitOfWork.Reviews.Update(review);

        // Recalculate rating
        var property = await _unitOfWork.Properties.GetByIdAsync(review.PropertyId);
        if (property != null)
        {
            var remaining = await _unitOfWork.Reviews.Query()
                .Where(r => r.PropertyId == review.PropertyId && r.Id != reviewId)
                .ToListAsync();

            property.AverageRating = remaining.Any() ? remaining.Average(r => r.Rating) : 0;
            property.TotalReviews = remaining.Count;
            _unitOfWork.Properties.Update(property);
        }

        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Review deleted");
    }
}
