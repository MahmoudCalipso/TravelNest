
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TravelNest.Application.DTOs.Common;
using TravelNest.Application.DTOs.Users;
using TravelNest.Application.Interfaces;
using TravelNest.Domain.Interfaces;

namespace TravelNest.Infrastructure.Services;
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorage;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileStorage = fileStorage;
    }

    public async Task<ApiResponse<UserProfileDto>> GetProfileAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.Query()
            .Include(u => u.Properties)
            .Include(u => u.Bookings)
            .Include(u => u.MediaPosts)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return ApiResponse<UserProfileDto>.FailResponse("User not found");

        return ApiResponse<UserProfileDto>.SuccessResponse(_mapper.Map<UserProfileDto>(user));
    }

    public async Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return ApiResponse<UserProfileDto>.FailResponse("User not found");

        _mapper.Map(dto, user);
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<UserProfileDto>.SuccessResponse(_mapper.Map<UserProfileDto>(user), "Profile updated");
    }

    public async Task<ApiResponse<string>> UploadProfilePictureAsync(Guid userId, IFormFile file)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return ApiResponse<string>.FailResponse("User not found");

        if (user.ProfilePictureUrl != null)
            await _fileStorage.DeleteFileAsync(user.ProfilePictureUrl);

        var url = await _fileStorage.UploadFileAsync(file, "profiles");
        user.ProfilePictureUrl = url;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<string>.SuccessResponse(url, "Profile picture updated");
    }

    public async Task<ApiResponse<PagedResponse<UserProfileDto>>> GetAllUsersAsync(PagedRequest request)
    {
        var query = _unitOfWork.Users.Query()
            .Include(u => u.Properties)
            .Include(u => u.Bookings)
            .Include(u => u.MediaPosts)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(search) ||
                u.LastName.ToLower().Contains(search) ||
                u.Email.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return ApiResponse<PagedResponse<UserProfileDto>>.SuccessResponse(new PagedResponse<UserProfileDto>
        {
            Data = _mapper.Map<IEnumerable<UserProfileDto>>(users),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }

    public async Task<ApiResponse<bool>> ToggleUserActiveAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return ApiResponse<bool>.FailResponse("User not found");

        user.IsActive = !user.IsActive;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true,
            user.IsActive ? "User activated" : "User deactivated");
    }

    public async Task<ApiResponse<bool>> DeleteUserAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return ApiResponse<bool>.FailResponse("User not found");

        user.IsDeleted = true;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "User deleted");
    }

    public async Task<ApiResponse<UserProfileDto>> GetPublicProfileAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.Query()
            .Include(u => u.Properties.Where(p => p.IsApproved && p.IsAvailable))
            .Include(u => u.MediaPosts)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user == null)
            return ApiResponse<UserProfileDto>.FailResponse("User not found");

        return ApiResponse<UserProfileDto>.SuccessResponse(_mapper.Map<UserProfileDto>(user));
    }
}
