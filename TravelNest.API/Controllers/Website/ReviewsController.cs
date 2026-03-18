using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelNest.API.Extensions;
using TravelNest.Application.DTOs.Common;
using TravelNest.Application.DTOs.Reviews;
using TravelNest.Application.Interfaces;

namespace TravelNest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    [Authorize(Policy = "Traveller")]
    public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
    {
        var result = await _reviewService.CreateAsync(User.GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("property/{propertyId}")]
    public async Task<IActionResult> GetByProperty(Guid propertyId, [FromQuery] PagedRequest request)
    {
        var result = await _reviewService.GetByPropertyAsync(propertyId, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AllAuthenticated")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _reviewService.DeleteAsync(id, User.GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
