using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelNest.API.Extensions;
using TravelNest.Application.DTOs.Common;
using TravelNest.Application.DTOs.Properties;
using TravelNest.Application.Interfaces;
using TravelNest.Domain.Enums;

namespace TravelNest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertiesController : ControllerBase
{
    private readonly IPropertyService _propertyService;

    public PropertiesController(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    [HttpPost]
    [Authorize(Policy = "Provider")]
    public async Task<IActionResult> Create([FromBody] CreatePropertyDto dto)
    {
        var result = await _propertyService.CreateAsync(User.GetUserId(), dto);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Provider")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePropertyDto dto)
    {
        var result = await _propertyService.UpdateAsync(User.GetUserId(), id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Provider")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _propertyService.DeleteAsync(User.GetUserId(), id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        Guid? currentUserId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : null;
        var result = await _propertyService.GetByIdAsync(id, currentUserId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] PropertySearchDto search)
    {
        Guid? currentUserId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : null;
        var result = await _propertyService.SearchAsync(search, currentUserId);
        return Ok(result);
    }

    [HttpGet("my-properties")]
    [Authorize(Policy = "Provider")]
    public async Task<IActionResult> GetMyProperties([FromQuery] PagedRequest request)
    {
        var result = await _propertyService.GetByProviderAsync(User.GetUserId(), request);
        return Ok(result);
    }

    [HttpGet("provider/{providerId}")]
    public async Task<IActionResult> GetByProvider(Guid providerId, [FromQuery] PagedRequest request)
    {
        var result = await _propertyService.GetByProviderAsync(providerId, request);
        return Ok(result);
    }

    [HttpPost("{id}/media")]
    [Authorize(Policy = "Provider")]
    public async Task<IActionResult> UploadMedia(Guid id, [FromForm] List<IFormFile> files,
        [FromQuery] MediaType mediaType = MediaType.Photo)
    {
        var result = await _propertyService.UploadMediaAsync(User.GetUserId(), id, files, mediaType);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("media/{mediaId}")]
    [Authorize(Policy = "Provider")]
    public async Task<IActionResult> DeleteMedia(Guid mediaId)
    {
        var result = await _propertyService.DeleteMediaAsync(User.GetUserId(), mediaId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{id}/favorite")]
    [Authorize(Policy = "AllAuthenticated")]
    public async Task<IActionResult> ToggleFavorite(Guid id)
    {
        var result = await _propertyService.ToggleFavoriteAsync(User.GetUserId(), id);
        return Ok(result);
    }

    [HttpGet("favorites")]
    [Authorize(Policy = "AllAuthenticated")]
    public async Task<IActionResult> GetFavorites([FromQuery] PagedRequest request)
    {
        var result = await _propertyService.GetFavoritesAsync(User.GetUserId(), request);
        return Ok(result);
    }

    // Admin endpoints
    [HttpGet("pending")]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<IActionResult> GetPending([FromQuery] PagedRequest request)
    {
        var result = await _propertyService.GetAllPendingAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}/approve")]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _propertyService.ApprovePropertyAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("{id}/reject")]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<IActionResult> Reject(Guid id)
    {
        var result = await _propertyService.RejectPropertyAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
