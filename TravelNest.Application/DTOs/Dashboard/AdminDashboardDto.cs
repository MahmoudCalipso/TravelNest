using System;
using System.Collections.Generic;
using System.Text;

namespace TravelNest.Application.DTOs.Dashboard
{
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalProviders { get; set; }
        public int TotalTravellers { get; set; }
        public int TotalProperties { get; set; }
        public int PendingApprovals { get; set; }
        public int TotalBookings { get; set; }
        public int ActiveBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<MonthlyStatDto> MonthlyBookings { get; set; } = new();
        public List<PropertyTypeStatDto> PropertyTypeStats { get; set; } = new();
    }

    public class MonthlyStatDto
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Revenue { get; set; }
    }

    public class PropertyTypeStatDto
    {
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
