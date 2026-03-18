using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using TravelNest.Application.DTOs.Common;
using TravelNest.Application.DTOs.Media;

namespace TravelNest.Application.Interfaces;

public interface IMediaService
{
    Task<ApiResponse<UserMediaDto>> CreatePostAsync(Guid userId, CreateUserMediaDto dto, IFormFile file);
    Task<ApiResponse<PagedResponse<UserMediaDto>>> GetFeedAsync(Guid? currentUserId, PagedRequest request);
    Task<ApiResponse<PagedResponse<UserMediaDto>>> GetByUserAsync(Guid userId, Guid? currentUserId, PagedRequest request);
    Task<ApiResponse<UserMediaDto>> GetByIdAsync(Guid mediaId, Guid? currentUserId);
    Task<ApiResponse<bool>> ToggleLikeAsync(Guid userId, Guid mediaId);
    Task<ApiResponse<MediaCommentDto>> AddCommentAsync(Guid userId, Guid mediaId, string content);
    Task<ApiResponse<bool>> DeletePostAsync(Guid userId, Guid mediaId);
    Task<ApiResponse<bool>> DeleteCommentAsync(Guid userId, Guid commentId);
    Task<ApiResponse<bool>> IncrementViewAsync(Guid mediaId);
}