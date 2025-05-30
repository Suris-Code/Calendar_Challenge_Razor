using System;

namespace Application.Features.Dashboard.Dtos
{
    public class DashboardDto
    {
        public int TotalWeeklyEvents { get; set; }
        public DayWithMostEventsDto DayWithMostEvents { get; set; }
        public DayWithMostHoursDto DayWithMostHours { get; set; }
        public DailyOccupancyPercentageDto[] DailyOccupancyPercentages { get; set; }
    }

    public class DayWithMostEventsDto
    {
        public DateTime Date { get; set; }
        public int EventCount { get; set; }
    }

    public class DayWithMostHoursDto
    {
        public DateTime Date { get; set; }
        public double TotalHours { get; set; }
    }

    public class DailyOccupancyPercentageDto
    {
        public DateTime Date { get; set; }
        public double OccupancyPercentage { get; set; } // 0-100 percentage
    }
} 