using TravelNest.Application.DTOs.Auth;
using TravelNest.Application.DTOs.Common;

namespace TravelNest.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto);
    Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto dto);
    Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
    Task<ApiResponse<bool>> LogoutAsync(Guid userId);
}