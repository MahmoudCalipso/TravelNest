
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TravelNest.Application.DTOs.Common;
using TravelNest.Application.DTOs.Messages;
using TravelNest.Application.Interfaces;
using TravelNest.Domain.Entities;
using TravelNest.Domain.Interfaces;

namespace TravelNest.Infrastructure.Services;

public class MessageService : IMessageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MessageService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<MessageDto>> SendAsync(Guid senderId, CreateMessageDto dto)
    {
        var receiver = await _unitOfWork.Users.GetByIdAsync(dto.ReceiverId);
        if (receiver == null)
            return ApiResponse<MessageDto>.FailResponse("Receiver not found");

        var message = new ContactMessage
        {
            SenderId = senderId,
            ReceiverId = dto.ReceiverId,
            BookingId = dto.BookingId,
            Subject = dto.Subject,
            Message = dto.Message
        };

        await _unitOfWork.ContactMessages.AddAsync(message);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.ContactMessages.Query()
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Include(m => m.Booking)
            .FirstOrDefaultAsync(m => m.Id == message.Id);

        return ApiResponse<MessageDto>.SuccessResponse(
            _mapper.Map<MessageDto>(created), "Message sent");
    }

    public async Task<ApiResponse<PagedResponse<MessageDto>>> GetInboxAsync(
        Guid userId, PagedRequest request)
    {
        var query = _unitOfWork.ContactMessages.Query()
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Include(m => m.Booking)
            .Where(m => m.ReceiverId == userId)
            .OrderByDescending(m => m.CreatedAt);

        var totalCount = await query.CountAsync();
        var messages = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return ApiResponse<PagedResponse<MessageDto>>.SuccessResponse(new PagedResponse<MessageDto>
        {
            Data = _mapper.Map<IEnumerable<MessageDto>>(messages),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }

    public async Task<ApiResponse<PagedResponse<MessageDto>>> GetSentAsync(
        Guid userId, PagedRequest request)
    {
        var query = _unitOfWork.ContactMessages.Query()
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Include(m => m.Booking)
            .Where(m => m.SenderId == userId)
            .OrderByDescending(m => m.CreatedAt);

        var totalCount = await query.CountAsync();
        var messages = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return ApiResponse<PagedResponse<MessageDto>>.SuccessResponse(new PagedResponse<MessageDto>
        {
            Data = _mapper.Map<IEnumerable<MessageDto>>(messages),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }

    public async Task<ApiResponse<bool>> MarkAsReadAsync(Guid messageId, Guid userId)
    {
        var message = await _unitOfWork.ContactMessages.GetByIdAsync(messageId);
        if (message == null || message.ReceiverId != userId)
            return ApiResponse<bool>.FailResponse("Message not found");

        message.IsRead = true;
        _unitOfWork.ContactMessages.Update(message);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Message marked as read");
    }

    public async Task<ApiResponse<MessageDto>> ReplyAsync(Guid messageId, Guid userId, string reply)
    {
        var message = await _unitOfWork.ContactMessages.Query()
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Include(m => m.Booking)
            .FirstOrDefaultAsync(m => m.Id == messageId && m.ReceiverId == userId);

        if (message == null)
            return ApiResponse<MessageDto>.FailResponse("Message not found");

        message.Reply = reply;
        message.IsRead = true;
        _unitOfWork.ContactMessages.Update(message);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<MessageDto>.SuccessResponse(
            _mapper.Map<MessageDto>(message), "Reply sent");
    }

    public async Task<ApiResponse<int>> GetUnreadCountAsync(Guid userId)
    {
        var count = await _unitOfWork.ContactMessages
            .CountAsync(m => m.ReceiverId == userId && !m.IsRead);

        return ApiResponse<int>.SuccessResponse(count);
    }
}
