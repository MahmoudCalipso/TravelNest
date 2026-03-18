using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelNest.API.Extensions;
using TravelNest.Application.DTOs.Bookings;
using TravelNest.Application.DTOs.Common;
using TravelNest.Application.Interfaces;

namespace TravelNest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    [Authorize(Policy = "Traveller")]
    public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
    {
        var result = await _bookingService.CreateAsync(User.GetUserId(), dto);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _bookingService.GetByIdAsync(id, User.GetUserId());
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("my-bookings")]
    [Authorize(Policy = "Traveller")]
    public async Task<IActionResult> GetMyBookings([FromQuery] PagedRequest request)
    {
        var result = await _bookingService.GetByTravellerAsync(User.GetUserId(), request);
        return Ok(result);
    }

    [HttpGet("provider-bookings")]
    [Authorize(Policy = "Provider")]
    public async Task<IActionResult> GetProviderBookings([FromQuery] PagedRequest request)
    {
        var result = await _bookingService.GetByProviderAsync(User.GetUserId(), request);
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
    {
        var result = await _bookingService.GetAllAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    [Authorize(Policy = "Provider")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateBookingStatusDto dto)
    {
        var result = await _bookingService.UpdateStatusAsync(id, User.GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}/cancel")]
    [Authorize(Policy = "Traveller")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] string? reason)
    {
        var result = await _bookingService.CancelBookingAsync(id, User.GetUserId(), reason);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
