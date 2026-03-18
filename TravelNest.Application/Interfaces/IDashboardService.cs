using TravelNest.Application.DTOs.Common;
using TravelNest.Application.DTOs.Dashboard;

namespace TravelNest.Application.Interfaces;

public interface IDashboardService
{
    Task<ApiResponse<AdminDashboardDto>> GetAdminDashboardAsync();
}