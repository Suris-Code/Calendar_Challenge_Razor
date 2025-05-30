namespace Web.Models;

public class DashboardStatisticsViewModel
{
    public int TotalEvents { get; set; }
    public DayEventInfoViewModel? DayWithMostEvents { get; set; }
    public DayHoursInfoViewModel? DayWithMostHours { get; set; }
    public DailyOccupancyViewModel? PeakOccupancy { get; set; }
    public List<DailyOccupancyViewModel> DailyOccupancy { get; set; } = new();
}

public class DayEventInfoViewModel
{
    public string Date { get; set; } = string.Empty;
    public int EventCount { get; set; }
}

public class DayHoursInfoViewModel
{
    public string Date { get; set; } = string.Empty;
    public double TotalHours { get; set; }
}

public class DailyOccupancyViewModel
{
    public string Date { get; set; } = string.Empty;
    public double OccupancyPercentage { get; set; }
}

public class DateRangeViewModel
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? WeekStart { get; set; }
} 