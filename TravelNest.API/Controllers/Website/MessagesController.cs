using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelNest.API.Extensions;
using TravelNest.Application.DTOs.Common;
using TravelNest.Application.DTOs.Messages;
using TravelNest.Application.Interfaces;

namespace TravelNest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AllAuthenticated")]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] CreateMessageDto dto)
    {
        var result = await _messageService.SendAsync(User.GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("inbox")]
    public async Task<IActionResult> GetInbox([FromQuery] PagedRequest request)
    {
        var result = await _messageService.GetInboxAsync(User.GetUserId(), request);
        return Ok(result);
    }

    [HttpGet("sent")]
    public async Task<IActionResult> GetSent([FromQuery] PagedRequest request)
    {
        var result = await _messageService.GetSentAsync(User.GetUserId(), request);
        return Ok(result);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var result = await _messageService.MarkAsReadAsync(id, User.GetUserId());
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{id}/reply")]
    public async Task<IActionResult> Reply(Guid id, [FromBody] string reply)
    {
        var result = await _messageService.ReplyAsync(id, User.GetUserId(), reply);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var result = await _messageService.GetUnreadCountAsync(User.GetUserId());
        return Ok(result);
    }
}
