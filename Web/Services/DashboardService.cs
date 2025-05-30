using Application.Features.Dashboard.Queries;
using Application.Features.Dashboard.Contracts;
using MediatR;
using Web.Models;

namespace Web.Services;

public interface IDashboardService
{
    Task<DashboardStatisticsViewModel> GetDashboardStatisticsAsync(DateRangeViewModel? dateRange = null);
    Task<int> GetWeeklyEventsTotalAsync(DateRangeViewModel? dateRange = null);
    Task<DayEventInfoViewModel> GetDayWithMostEventsAsync(DateRangeViewModel? dateRange = null);
    Task<DayHoursInfoViewModel> GetDayWithMostHoursAsync(DateRangeViewModel? dateRange = null);
    Task<List<DailyOccupancyViewModel>> GetDailyOccupancyAsync(DateRangeViewModel? dateRange = null);
}

public class DashboardService : IDashboardService
{
    private readonly ISender _mediator;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(ISender mediator, ILogger<DashboardService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<DashboardStatisticsViewModel> GetDashboardStatisticsAsync(DateRangeViewModel? dateRange = null)
    {
        try
        {
            // Debug logging
            _logger.LogInformation("DashboardService - GetDashboardStatisticsAsync called with StartDate: {StartDate}, EndDate: {EndDate}, WeekStart: {WeekStart}", 
                dateRange?.StartDate, dateRange?.EndDate, dateRange?.WeekStart);

            // Use the main dashboard query that fetches all data efficiently
            var request = new GetDashboardStatisticsRequest
            {
                StartDate = dateRange?.StartDate,
                EndDate = dateRange?.EndDate,
                WeekStart = dateRange?.WeekStart
            };

            var query = new GetDashboardStatisticsQuery(request);
            var response = await _mediator.Send(query);

            _logger.LogInformation("DashboardService - Query response succeeded: {Succeeded}, Data is null: {DataIsNull}", 
                response.Result.Succeeded, response.Data == null);

            if (response.Result.Succeeded && response.Data != null)
            {
                var data = response.Data;
                
                _logger.LogInformation("DashboardService - Raw data: TotalWeeklyEvents: {TotalEvents}, DayWithMostEvents: {DayWithMostEvents}, DayWithMostHours: {DayWithMostHours}", 
                    data.TotalWeeklyEvents, 
                    $"{data.DayWithMostEvents?.Date:yyyy-MM-dd} ({data.DayWithMostEvents?.EventCount})",
                    $"{data.DayWithMostHours?.Date:yyyy-MM-dd} ({data.DayWithMostHours?.TotalHours})");
                
                // Convert daily occupancy to view models
                var dailyOccupancy = data.DailyOccupancyPercentages?.Select(d => new DailyOccupancyViewModel
                {
                    Date = d.Date.ToString("yyyy-MM-dd"),
                    OccupancyPercentage = d.OccupancyPercentage
                }).ToList() ?? new List<DailyOccupancyViewModel>();

                // Find the busiest day based on occupancy
                var peakOccupancy = dailyOccupancy
                    .OrderByDescending(d => d.OccupancyPercentage)
                    .FirstOrDefault();

                var result = new DashboardStatisticsViewModel
                {
                    TotalEvents = data.TotalWeeklyEvents,
                    DayWithMostEvents = new DayEventInfoViewModel
                    {
                        Date = data.DayWithMostEvents.Date.ToString("yyyy-MM-dd"),
                        EventCount = data.DayWithMostEvents.EventCount
                    },
                    DayWithMostHours = new DayHoursInfoViewModel
                    {
                        Date = data.DayWithMostHours.Date.ToString("yyyy-MM-dd"),
                        TotalHours = data.DayWithMostHours.TotalHours
                    },
                    PeakOccupancy = peakOccupancy,
                    DailyOccupancy = dailyOccupancy
                };

                _logger.LogInformation("DashboardService - Returning result with TotalEvents: {TotalEvents}", result.TotalEvents);
                return result;
            }

            return new DashboardStatisticsViewModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard statistics");
            return new DashboardStatisticsViewModel();
        }
    }

    public async Task<int> GetWeeklyEventsTotalAsync(DateRangeViewModel? dateRange = null)
    {
        try
        {
            var query = new GetWeeklyEventsTotalQuery(
                dateRange?.StartDate,
                dateRange?.EndDate,
                dateRange?.WeekStart
            );
            return await _mediator.Send(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weekly events total");
            return 0;
        }
    }

    public async Task<DayEventInfoViewModel> GetDayWithMostEventsAsync(DateRangeViewModel? dateRange = null)
    {
        try
        {
            var query = new GetDayWithMostEventsQuery(
                dateRange?.StartDate,
                dateRange?.EndDate,
                dateRange?.WeekStart
            );
            var result = await _mediator.Send(query);
            
            return new DayEventInfoViewModel
            {
                Date = result.Date.ToString("yyyy-MM-dd"),
                EventCount = result.EventCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching day with most events");
            return new DayEventInfoViewModel { Date = "Unknown", EventCount = 0 };
        }
    }

    public async Task<DayHoursInfoViewModel> GetDayWithMostHoursAsync(DateRangeViewModel? dateRange = null)
    {
        try
        {
            var query = new GetDayWithMostHoursQuery(
                dateRange?.StartDate,
                dateRange?.EndDate,
                dateRange?.WeekStart
            );
            var result = await _mediator.Send(query);
            
            return new DayHoursInfoViewModel
            {
                Date = result.Date.ToString("yyyy-MM-dd"),
                TotalHours = result.TotalHours
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching day with most hours");
            return new DayHoursInfoViewModel { Date = "Unknown", TotalHours = 0 };
        }
    }

    public async Task<List<DailyOccupancyViewModel>> GetDailyOccupancyAsync(DateRangeViewModel? dateRange = null)
    {
        try
        {
            var query = new GetDailyOccupancyPercentagesQuery(
                dateRange?.StartDate,
                dateRange?.EndDate,
                dateRange?.WeekStart
            );
            var result = await _mediator.Send(query);
            
            return result.Select(d => new DailyOccupancyViewModel
            {
                Date = d.Date.ToString("yyyy-MM-dd"),
                OccupancyPercentage = d.OccupancyPercentage
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching daily occupancy");
            return new List<DailyOccupancyViewModel>();
        }
    }
} 