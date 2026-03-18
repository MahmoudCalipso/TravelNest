using System;
using System.Collections.Generic;
using System.Text;
using TravelNest.Application.DTOs.Common;
using TravelNest.Application.DTOs.Messages;

namespace TravelNest.Application.Interfaces;

public interface IMessageService
{
    Task<ApiResponse<MessageDto>> SendAsync(Guid senderId, CreateMessageDto dto);
    Task<ApiResponse<PagedResponse<MessageDto>>> GetInboxAsync(Guid userId, PagedRequest request);
    Task<ApiResponse<PagedResponse<MessageDto>>> GetSentAsync(Guid userId, PagedRequest request);
    Task<ApiResponse<bool>> MarkAsReadAsync(Guid messageId, Guid userId);
    Task<ApiResponse<MessageDto>> ReplyAsync(Guid messageId, Guid userId, string reply);
    Task<ApiResponse<int>> GetUnreadCountAsync(Guid userId);
}