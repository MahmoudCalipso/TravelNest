using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using TravelNest.Application.DTOs.Common;
using TravelNest.Application.DTOs.Users;

namespace TravelNest.Application.Interfaces;

public interface IUserService
{
    Task<ApiResponse<UserProfileDto>> GetProfileAsync(Guid userId);
    Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
    Task<ApiResponse<string>> UploadProfilePictureAsync(Guid userId, IFormFile file);
    Task<ApiResponse<PagedResponse<UserProfileDto>>> GetAllUsersAsync(PagedRequest request);
    Task<ApiResponse<bool>> ToggleUserActiveAsync(Guid userId);
    Task<ApiResponse<bool>> DeleteUserAsync(Guid userId);
    Task<ApiResponse<UserProfileDto>> GetPublicProfileAsync(Guid userId);
}
