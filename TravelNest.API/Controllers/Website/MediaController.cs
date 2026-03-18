using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelNest.API.Extensions;
using TravelNest.Application.DTOs.Common;
using TravelNest.Application.DTOs.Media;
using TravelNest.Application.Interfaces;

namespace TravelNest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;

    public MediaController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    [HttpPost]
    [Authorize(Policy = "AllAuthenticated")]
    public async Task<IActionResult> CreatePost([FromForm] CreateUserMediaDto dto, IFormFile file)
    {
        var result = await _mediaService.CreatePostAsync(User.GetUserId(), dto, file);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed([FromQuery] PagedRequest request)
    {
        Guid? currentUserId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : null;
        var result = await _mediaService.GetFeedAsync(currentUserId, request);
        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(Guid userId, [FromQuery] PagedRequest request)
    {
        Guid? currentUserId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : null;
        var result = await _mediaService.GetByUserAsync(userId, currentUserId, request);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        Guid? currentUserId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : null;
        var result = await _mediaService.GetByIdAsync(id, currentUserId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{id}/like")]
    [Authorize(Policy = "AllAuthenticated")]
    public async Task<IActionResult> ToggleLike(Guid id)
    {
        var result = await _mediaService.ToggleLikeAsync(User.GetUserId(), id);
        return Ok(result);
    }

    [HttpPost("{id}/comment")]
    [Authorize(Policy = "AllAuthenticated")]
    public async Task<IActionResult> AddComment(Guid id, [FromBody] string content)
    {
        var result = await _mediaService.AddCommentAsync(User.GetUserId(), id, content);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AllAuthenticated")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        var result = await _mediaService.DeletePostAsync(User.GetUserId(), id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("comment/{commentId}")]
    [Authorize(Policy = "AllAuthenticated")]
    public async Task<IActionResult> DeleteComment(Guid commentId)
    {
        var result = await _mediaService.DeleteCommentAsync(User.GetUserId(), commentId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/view")]
    public async Task<IActionResult> IncrementView(Guid id)
    {
        var result = await _mediaService.IncrementViewAsync(id);
        return Ok(result);
    }
}
