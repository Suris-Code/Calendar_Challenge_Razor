using Application.Features.Appointments.Contracts;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services;
using static Web.Services.AuthorizationExtensions;

namespace Web.Controllers;

[Authorize]
public class CalendarController : Controller
{
    private readonly ILogger<CalendarController> _logger;
    private readonly ICalendarService _calendarService;
    private readonly IDashboardService _dashboardService;

    public CalendarController(ILogger<CalendarController> logger, ICalendarService calendarService, IDashboardService dashboardService)
    {
        _logger = logger;
        _calendarService = calendarService;
        _dashboardService = dashboardService;
    }

    [AuthorizePolicies(Policy.LoggedIn)]
    public async Task<IActionResult> Index()
    {
        try
        {
            // Get current week's statistics for the dashboard cards
            var currentWeekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var currentWeekEnd = currentWeekStart.AddDays(6);
            
            var dateRange = new DateRangeViewModel
            {
                StartDate = currentWeekStart,
                EndDate = currentWeekEnd,
                WeekStart = currentWeekStart
            };

            // Use individual methods for better control and debugging
            var totalEvents = await _dashboardService.GetWeeklyEventsTotalAsync(dateRange);
            var dayWithMostEvents = await _dashboardService.GetDayWithMostEventsAsync(dateRange);
            var dayWithMostHours = await _dashboardService.GetDayWithMostHoursAsync(dateRange);
            var dailyOccupancy = await _dashboardService.GetDailyOccupancyAsync(dateRange);
            
            // Find peak occupancy
            var peakOccupancy = dailyOccupancy?
                .OrderByDescending(d => d.OccupancyPercentage)
                .FirstOrDefault();

            var statistics = new DashboardStatisticsViewModel
            {
                TotalEvents = totalEvents,
                DayWithMostEvents = dayWithMostEvents,
                DayWithMostHours = dayWithMostHours,
                PeakOccupancy = peakOccupancy,
                DailyOccupancy = dailyOccupancy
            };

            return View(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading calendar with dashboard statistics");
            return View(new DashboardStatisticsViewModel());
        }
    }

    [HttpGet]
    [AuthorizePolicies(Policy.LoggedIn)]
    public async Task<JsonResult> GetEvents([FromQuery] GetAppointmentsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var events = await _calendarService.GetCalendarEventsAsync(request);
            return Json(new { success = true, data = events });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching calendar events");
            return Json(new { success = false, message = "Error fetching events" });
        }
    }

    [HttpPost]
    [AuthorizePolicies(Policy.LoggedIn)]
    public async Task<JsonResult> CreateEvent([FromBody] CreateEventViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data provided" });
            }

            var createdEvent = await _calendarService.CreateEventAsync(model);
            
            if (createdEvent != null)
            {
                return Json(new { success = true, data = new { id = createdEvent.Id } });
            }

            return Json(new { success = false, message = "Error creating event" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating calendar event");
            return Json(new { success = false, message = "Error creating event" });
        }
    }

    [HttpPut]
    [AuthorizePolicies(Policy.LoggedIn)]
    public async Task<JsonResult> UpdateEvent([FromBody] UpdateEventViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data provided" });
            }

            var updatedEvent = await _calendarService.UpdateEventAsync(model);
            
            if (updatedEvent != null)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Error updating event" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating calendar event");
            return Json(new { success = false, message = "Error updating event" });
        }
    }

    [HttpDelete]
    [AuthorizePolicies(Policy.AppointmentOwner)]
    public async Task<JsonResult> DeleteEvent(int id, CancellationToken cancellationToken)
    {
        try
        {
            var success = await _calendarService.DeleteEventAsync(id);
            
            if (success)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Error deleting event" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting calendar event");
            return Json(new { success = false, message = "Error deleting event" });
        }
    }

    [HttpGet]
    [AuthorizePolicies(Policy.LoggedIn)]
    public async Task<JsonResult> GetDashboardStatistics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] DateTime? weekStart)
    {
        try
        {
            // Debug logging
            _logger.LogInformation("Dashboard Statistics Request - StartDate: {StartDate}, EndDate: {EndDate}, WeekStart: {WeekStart}", 
                startDate, endDate, weekStart);

            var dateRange = new DateRangeViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                WeekStart = weekStart
            };

            // Use individual methods for better control and debugging
            var totalEvents = await _dashboardService.GetWeeklyEventsTotalAsync(dateRange);
            var dayWithMostEvents = await _dashboardService.GetDayWithMostEventsAsync(dateRange);
            var dayWithMostHours = await _dashboardService.GetDayWithMostHoursAsync(dateRange);
            var dailyOccupancy = await _dashboardService.GetDailyOccupancyAsync(dateRange);
            
            _logger.LogInformation("Individual method results - TotalEvents: {TotalEvents}, DayWithMostEvents: {DayWithMostEvents}, DayWithMostHours: {DayWithMostHours}, DailyOccupancy count: {DailyOccupancyCount}", 
                totalEvents, 
                $"{dayWithMostEvents?.Date} ({dayWithMostEvents?.EventCount})",
                $"{dayWithMostHours?.Date} ({dayWithMostHours?.TotalHours})",
                dailyOccupancy?.Count ?? 0);
            
            // Find peak occupancy
            var peakOccupancy = dailyOccupancy?
                .OrderByDescending(d => d.OccupancyPercentage)
                .FirstOrDefault();

            var statistics = new DashboardStatisticsViewModel
            {
                TotalEvents = totalEvents,
                DayWithMostEvents = dayWithMostEvents,
                DayWithMostHours = dayWithMostHours,
                PeakOccupancy = peakOccupancy,
                DailyOccupancy = dailyOccupancy
            };
            
            _logger.LogInformation("Dashboard Statistics Response - TotalEvents: {TotalEvents}", statistics.TotalEvents);
            
            return Json(new { success = true, data = statistics });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard statistics for calendar");
            return Json(new { success = false, message = "Error fetching statistics" });
        }
    }
} 