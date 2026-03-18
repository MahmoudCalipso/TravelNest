
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TravelNest.Application.DTOs.Common;
using TravelNest.Application.DTOs.Media;
using TravelNest.Application.Interfaces;
using TravelNest.Domain.Entities;
using TravelNest.Domain.Enums;
using TravelNest.Domain.Interfaces;

namespace TravelNest.Infrastructure.Services;

public class MediaService : IMediaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorage;

    public MediaService(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileStorage = fileStorage;
    }

    public async Task<ApiResponse<UserMediaDto>> CreatePostAsync(
        Guid userId, CreateUserMediaDto dto, IFormFile file)
    {
        var url = await _fileStorage.UploadFileAsync(file, $"media/{userId}");
        var thumbnail = dto.Type != MediaType.Photo
            ? await _fileStorage.GenerateThumbnailAsync(url)
            : url;

        var media = new UserMedia
        {
            UserId = userId,
            Url = url,
            ThumbnailUrl = thumbnail,
            Type = dto.Type,
            Caption = dto.Caption,
            Location = dto.Location,
            PropertyId = dto.PropertyId
        };

        await _unitOfWork.UserMedia.AddAsync(media);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.UserMedia.Query()
            .Include(m => m.User)
            .Include(m => m.Property)
            .FirstOrDefaultAsync(m => m.Id == media.Id);

        return ApiResponse<UserMediaDto>.SuccessResponse(
            _mapper.Map<UserMediaDto>(created), "Post created");
    }

    public async Task<ApiResponse<PagedResponse<UserMediaDto>>> GetFeedAsync(
        Guid? currentUserId, PagedRequest request)
    {
        var query = _unitOfWork.UserMedia.Query()
            .Include(m => m.User)
            .Include(m => m.Property)
            .Include(m => m.Comments.OrderByDescending(c => c.CreatedAt).Take(3))
                .ThenInclude(c => c.User)
            .OrderByDescending(m => m.CreatedAt)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(m =>
                (m.Caption != null && m.Caption.ToLower().Contains(term)) ||
                (m.Location != null && m.Location.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();
        var media = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var likedMediaIds = new HashSet<Guid>();
        if (currentUserId.HasValue)
        {
            likedMediaIds = (await _unitOfWork.MediaLikes
                .FindAsync(l => l.UserId == currentUserId.Value))
                .Select(l => l.UserMediaId)
                .ToHashSet();
        }

        var dtos = media.Select(m =>
        {
            var dto = _mapper.Map<UserMediaDto>(m);
            dto.IsLikedByCurrentUser = likedMediaIds.Contains(m.Id);
            return dto;
        });

        return ApiResponse<PagedResponse<UserMediaDto>>.SuccessResponse(new PagedResponse<UserMediaDto>
        {
            Data = dtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }

    public async Task<ApiResponse<PagedResponse<UserMediaDto>>> GetByUserAsync(
        Guid userId, Guid? currentUserId, PagedRequest request)
    {
        var query = _unitOfWork.UserMedia.Query()
            .Include(m => m.User)
            .Include(m => m.Property)
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt);

        var totalCount = await query.CountAsync();
        var media = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var likedMediaIds = new HashSet<Guid>();
        if (currentUserId.HasValue)
        {
            likedMediaIds = (await _unitOfWork.MediaLikes
                .FindAsync(l => l.UserId == currentUserId.Value))
                .Select(l => l.UserMediaId)
                .ToHashSet();
        }

        var dtos = media.Select(m =>
        {
            var dto = _mapper.Map<UserMediaDto>(m);
            dto.IsLikedByCurrentUser = likedMediaIds.Contains(m.Id);
            return dto;
        });

        return ApiResponse<PagedResponse<UserMediaDto>>.SuccessResponse(new PagedResponse<UserMediaDto>
        {
            Data = dtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }

    public async Task<ApiResponse<UserMediaDto>> GetByIdAsync(Guid mediaId, Guid? currentUserId)
    {
        var media = await _unitOfWork.UserMedia.Query()
            .Include(m => m.User)
            .Include(m => m.Property)
            .Include(m => m.Comments.OrderByDescending(c => c.CreatedAt))
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(m => m.Id == mediaId);

        if (media == null)
            return ApiResponse<UserMediaDto>.FailResponse("Media not found");

        var dto = _mapper.Map<UserMediaDto>(media);

        if (currentUserId.HasValue)
        {
            dto.IsLikedByCurrentUser = await _unitOfWork.MediaLikes
                .AnyAsync(l => l.UserId == currentUserId.Value && l.UserMediaId == mediaId);
        }

        return ApiResponse<UserMediaDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<bool>> ToggleLikeAsync(Guid userId, Guid mediaId)
    {
        var existing = await _unitOfWork.MediaLikes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.UserMediaId == mediaId);

        var media = await _unitOfWork.UserMedia.GetByIdAsync(mediaId);
        if (media == null)
            return ApiResponse<bool>.FailResponse("Media not found");

        if (existing != null)
        {
            _unitOfWork.MediaLikes.Remove(existing);
            media.LikesCount = Math.Max(0, media.LikesCount - 1);
        }
        else
        {
            await _unitOfWork.MediaLikes.AddAsync(new MediaLike
            {
                UserId = userId,
                UserMediaId = mediaId
            });
            media.LikesCount++;
        }

        _unitOfWork.UserMedia.Update(media);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(
            existing == null,
            existing == null ? "Liked" : "Unliked");
    }

    public async Task<ApiResponse<MediaCommentDto>> AddCommentAsync(
        Guid userId, Guid mediaId, string content)
    {
        var media = await _unitOfWork.UserMedia.GetByIdAsync(mediaId);
        if (media == null)
            return ApiResponse<MediaCommentDto>.FailResponse("Media not found");

        var comment = new MediaComment
        {
            UserId = userId,
            UserMediaId = mediaId,
            Content = content
        };

        await _unitOfWork.MediaComments.AddAsync(comment);

        media.CommentsCount++;
        _unitOfWork.UserMedia.Update(media);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.MediaComments.Query()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == comment.Id);

        return ApiResponse<MediaCommentDto>.SuccessResponse(
            _mapper.Map<MediaCommentDto>(created), "Comment added");
    }

    public async Task<ApiResponse<bool>> DeletePostAsync(Guid userId, Guid mediaId)
    {
        var media = await _unitOfWork.UserMedia.GetByIdAsync(mediaId);
        if (media == null)
            return ApiResponse<bool>.FailResponse("Media not found");

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (media.UserId != userId && user?.Role != UserRole.SuperAdmin)
            return ApiResponse<bool>.FailResponse("Access denied");

        await _fileStorage.DeleteFileAsync(media.Url);
        media.IsDeleted = true;
        _unitOfWork.UserMedia.Update(media);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Post deleted");
    }

    public async Task<ApiResponse<bool>> DeleteCommentAsync(Guid userId, Guid commentId)
    {
        var comment = await _unitOfWork.MediaComments.GetByIdAsync(commentId);
        if (comment == null)
            return ApiResponse<bool>.FailResponse("Comment not found");

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (comment.UserId != userId && user?.Role != UserRole.SuperAdmin)
            return ApiResponse<bool>.FailResponse("Access denied");

        comment.IsDeleted = true;
        _unitOfWork.MediaComments.Update(comment);

        var media = await _unitOfWork.UserMedia.GetByIdAsync(comment.UserMediaId);
        if (media != null)
        {
            media.CommentsCount = Math.Max(0, media.CommentsCount - 1);
            _unitOfWork.UserMedia.Update(media);
        }

        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Comment deleted");
    }

    public async Task<ApiResponse<bool>> IncrementViewAsync(Guid mediaId)
    {
        var media = await _unitOfWork.UserMedia.GetByIdAsync(mediaId);
        if (media == null)
            return ApiResponse<bool>.FailResponse("Media not found");

        media.ViewsCount++;
        _unitOfWork.UserMedia.Update(media);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true);
    }
}
