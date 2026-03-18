
using Microsoft.EntityFrameworkCore;
using TravelNest.Application.DTOs.Common;
using TravelNest.Application.DTOs.Dashboard;
using TravelNest.Application.Interfaces;
using TravelNest.Domain.Enums;
using TravelNest.Domain.Interfaces;

namespace TravelNest.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<AdminDashboardDto>> GetAdminDashboardAsync()
    {
        var dashboard = new AdminDashboardDto
        {
            TotalUsers = await _unitOfWork.Users.CountAsync(),
            TotalProviders = await _unitOfWork.Users.CountAsync(u => u.Role == UserRole.Provider),
            TotalTravellers = await _unitOfWork.Users.CountAsync(u => u.Role == UserRole.Traveller),
            TotalProperties = await _unitOfWork.Properties.CountAsync(),
            PendingApprovals = await _unitOfWork.Properties.CountAsync(p => !p.IsApproved),
            TotalBookings = await _unitOfWork.Bookings.CountAsync(),
            ActiveBookings = await _unitOfWork.Bookings
                .CountAsync(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending),
            TotalRevenue = await _unitOfWork.Payments.Query()
                .Where(p => p.Status == PaymentStatus.Paid)
                .SumAsync(p => p.Amount)
        };

        // Monthly stats for last 12 months
        var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-12);
        var monthlyBookings = await _unitOfWork.Bookings.Query()
            .Where(b => b.CreatedAt >= twelveMonthsAgo)
            .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
            .Select(g => new MonthlyStatDto
            {
                Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                Count = g.Count(),
                Revenue = g.Sum(b => b.TotalPrice)
            })
            .OrderBy(m => m.Month)
            .ToListAsync();

        dashboard.MonthlyBookings = monthlyBookings;

        // Property type stats
        var typeStats = await _unitOfWork.Properties.Query()
            .GroupBy(p => p.Type)
            .Select(g => new PropertyTypeStatDto
            {
                Type = g.Key.ToString(),
                Count = g.Count()
            })
            .ToListAsync();

        dashboard.PropertyTypeStats = typeStats;

        return ApiResponse<AdminDashboardDto>.SuccessResponse(dashboard);
    }
}

