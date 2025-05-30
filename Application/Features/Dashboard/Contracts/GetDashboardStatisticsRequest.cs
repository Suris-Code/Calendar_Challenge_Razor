using System;

namespace Application.Features.Dashboard.Contracts
{
    public class GetDashboardStatisticsRequest
    {
        public DateTime? WeekStart { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
} 