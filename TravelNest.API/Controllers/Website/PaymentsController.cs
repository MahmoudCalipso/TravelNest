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
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    [Authorize(Policy = "Provider")]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
    {
        var result = await _paymentService.CreatePaymentAsync(User.GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}/status")]
    [Authorize(Policy = "Provider")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdatePaymentStatusDto dto)
    {
        var result = await _paymentService.UpdatePaymentStatusAsync(id, User.GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("booking/{bookingId}")]
    public async Task<IActionResult> GetByBooking(Guid bookingId)
    {
        var result = await _paymentService.GetByBookingAsync(bookingId, User.GetUserId());
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
    {
        var result = await _paymentService.GetAllPaymentsAsync(request);
        return Ok(result);
    }
}
