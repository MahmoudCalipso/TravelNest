using System;
using System.Collections.Generic;
using System.Text;
using TravelNest.Application.DTOs.Common;
using TravelNest.Application.DTOs.Reviews;

namespace TravelNest.Application.Interfaces;

public interface IReviewService
{
    Task<ApiResponse<ReviewDto>> CreateAsync(Guid travellerId, CreateReviewDto dto);
    Task<ApiResponse<PagedResponse<ReviewDto>>> GetByPropertyAsync(Guid propertyId, PagedRequest request);
    Task<ApiResponse<bool>> DeleteAsync(Guid reviewId, Guid userId);
}