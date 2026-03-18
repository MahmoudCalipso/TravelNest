using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelNest.API.Extensions;
using TravelNest.Application.DTOs.Common;
using TravelNest.Application.DTOs.Users;
using TravelNest.Application.Interfaces;

namespace TravelNest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("profile")]
    [Authorize(Policy = "AllAuthenticated")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _userService.GetProfileAsync(User.GetUserId());
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("profile")]
    [Authorize(Policy = "AllAuthenticated")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var result = await _userService.UpdateProfileAsync(User.GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("profile/picture")]
    [Authorize(Policy = "AllAuthenticated")]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        var result = await _userService.UploadProfilePictureAsync(User.GetUserId(), file);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}/public")]
    public async Task<IActionResult> GetPublicProfile(Guid id)
    {
        var result = await _userService.GetPublicProfileAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // Admin endpoints
    [HttpGet]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<IActionResult> GetAllUsers([FromQuery] PagedRequest request)
    {
        var result = await _userService.GetAllUsersAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}/toggle-active")]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        var result = await _userService.ToggleUserActiveAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var result = await _userService.DeleteUserAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
