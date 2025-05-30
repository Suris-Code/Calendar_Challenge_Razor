using Application.Features.Appointments.Commands;
using Application.Features.Appointments.Contracts;
using Application.Features.Appointments.Queries;
using Domain.Enums;
using MediatR;
using Web.Models;

namespace Web.Services;

public interface ICalendarService
{
    Task<IEnumerable<CalendarEventViewModel>> GetCalendarEventsAsync(GetAppointmentsRequest? request = null);
    Task<CalendarEventViewModel?> CreateEventAsync(CreateEventViewModel model);
    Task<CalendarEventViewModel?> UpdateEventAsync(UpdateEventViewModel model);
    Task<bool> DeleteEventAsync(int eventId);
}

public class CalendarService : ICalendarService
{
    private readonly ISender _mediator;
    private readonly ILogger<CalendarService> _logger;

    public CalendarService(ISender mediator, ILogger<CalendarService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<IEnumerable<CalendarEventViewModel>> GetCalendarEventsAsync(GetAppointmentsRequest? request = null)
    {
        try
        {
            request ??= new GetAppointmentsRequest();
            var query = new GetAppointmentsQuery(request);
            var response = await _mediator.Send(query);

            if (response.Result.Succeeded && response.Data != null)
            {
                return response.Data.Select(appointment => new CalendarEventViewModel
                {
                    Id = appointment.Id.ToString(),
                    Title = appointment.Title,
                    Start = appointment.StartTime.ToString("yyyy-MM-dd HH:mm"),
                    End = appointment.EndTime.ToString("yyyy-MM-dd HH:mm"),
                    Description = appointment.Description,
                    Email = appointment.UserEmail,
                    Location = appointment.Location,
                    IsConfirmed = appointment.IsConfirmed == YesNo.Yes,
                    IsCancelled = appointment.IsCancelled == YesNo.Yes
                });
            }

            return Enumerable.Empty<CalendarEventViewModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching calendar events");
            return Enumerable.Empty<CalendarEventViewModel>();
        }
    }

    public async Task<CalendarEventViewModel?> CreateEventAsync(CreateEventViewModel model)
    {
        try
        {
            var request = new CreateAppointmentRequest
            {
                Title = model.Title,
                Description = model.Description ?? "No description",
                StartTime = DateTime.Parse(model.Start),
                EndTime = DateTime.Parse(model.End),
                Location = model.Location,
                IsConfirmed = YesNo.No,
                SendReminder = YesNo.Yes
            };

            var command = new CreateAppointmentCommand(request);
            var response = await _mediator.Send(command);

            if (response.Result.Succeeded && response.Id > 0)
            {
                // Fetch the created appointment to return complete data
                var getQuery = new GetAppointmentQuery(new GetAppointmentRequest { Id = response.Id });
                var getResponse = await _mediator.Send(getQuery);

                if (getResponse.Result.Succeeded && getResponse.Data != null)
                {
                    return new CalendarEventViewModel
                    {
                        Id = getResponse.Data.Id.ToString(),
                        Title = getResponse.Data.Title,
                        Start = getResponse.Data.StartTime.ToString("yyyy-MM-dd HH:mm"),
                        End = getResponse.Data.EndTime.ToString("yyyy-MM-dd HH:mm"),
                        Description = getResponse.Data.Description,
                        Location = getResponse.Data.Location,
                        IsConfirmed = getResponse.Data.IsConfirmed == YesNo.Yes,
                        IsCancelled = getResponse.Data.IsCancelled == YesNo.Yes
                    };
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating calendar event");
            return null;
        }
    }

    public async Task<CalendarEventViewModel?> UpdateEventAsync(UpdateEventViewModel model)
    {
        try
        {
            // First get the current appointment to preserve existing data
            var getQuery = new GetAppointmentQuery(new GetAppointmentRequest { Id = model.Id });
            var currentResponse = await _mediator.Send(getQuery);

            if (!currentResponse.Result.Succeeded || currentResponse.Data == null)
            {
                return null;
            }

            var current = currentResponse.Data;
            var request = new UpdateAppointmentRequest
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description ?? current.Description,
                StartTime = DateTime.Parse(model.Start),
                EndTime = DateTime.Parse(model.End),
                Location = model.Location ?? current.Location,
                IsConfirmed = current.IsConfirmed,
                IsCancelled = current.IsCancelled,
                CancellationReason = current.CancellationReason,
                MeetingLink = current.MeetingLink,
                SendReminder = current.SendReminder
            };

            var command = new UpdateAppointmentCommand(request);
            var response = await _mediator.Send(command);

            if (response.Result.Succeeded)
            {
                // Fetch the updated appointment to return complete data
                var updatedResponse = await _mediator.Send(getQuery);
                if (updatedResponse.Result.Succeeded && updatedResponse.Data != null)
                {
                    return new CalendarEventViewModel
                    {
                        Id = updatedResponse.Data.Id.ToString(),
                        Title = updatedResponse.Data.Title,
                        Start = updatedResponse.Data.StartTime.ToString("yyyy-MM-dd HH:mm"),
                        End = updatedResponse.Data.EndTime.ToString("yyyy-MM-dd HH:mm"),
                        Description = updatedResponse.Data.Description,
                        Location = updatedResponse.Data.Location,
                        IsConfirmed = updatedResponse.Data.IsConfirmed == YesNo.Yes,
                        IsCancelled = updatedResponse.Data.IsCancelled == YesNo.Yes
                    };
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating calendar event");
            return null;
        }
    }

    public async Task<bool> DeleteEventAsync(int eventId)
    {
        try
        {
            var command = new DeleteAppointmentCommand(new DeleteAppointmentRequest { Id = eventId });
            var response = await _mediator.Send(command);
            return response.Result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting calendar event");
            return false;
        }
    }
} 