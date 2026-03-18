using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using TravelNest.Application.DTOs.Common;
using TravelNest.Application.DTOs.Properties;

namespace TravelNest.Application.Interfaces;

public interface IPropertyService
{
    Task<ApiResponse<PropertyDto>> CreateAsync(Guid providerId, CreatePropertyDto dto);
    Task<ApiResponse<PropertyDto>> UpdateAsync(Guid providerId, Guid propertyId, UpdatePropertyDto dto);
    Task<ApiResponse<bool>> DeleteAsync(Guid providerId, Guid propertyId);
    Task<ApiResponse<PropertyDto>> GetByIdAsync(Guid propertyId, Guid? currentUserId = null);
    Task<ApiResponse<PagedResponse<PropertyDto>>> SearchAsync(PropertySearchDto search, Guid? currentUserId = null);
    Task<ApiResponse<PagedResponse<PropertyDto>>> GetByProviderAsync(Guid providerId, PagedRequest request);
    Task<ApiResponse<List<PropertyMediaDto>>> UploadMediaAsync(Guid providerId, Guid propertyId, List<IFormFile> files, Domain.Enums.MediaType mediaType);
    Task<ApiResponse<bool>> DeleteMediaAsync(Guid providerId, Guid mediaId);
    Task<ApiResponse<bool>> ApprovePropertyAsync(Guid propertyId);
    Task<ApiResponse<bool>> RejectPropertyAsync(Guid propertyId);
    Task<ApiResponse<bool>> ToggleFavoriteAsync(Guid userId, Guid propertyId);
    Task<ApiResponse<PagedResponse<PropertyDto>>> GetFavoritesAsync(Guid userId, PagedRequest request);
    Task<ApiResponse<PagedResponse<PropertyDto>>> GetAllPendingAsync(PagedRequest request);
}
