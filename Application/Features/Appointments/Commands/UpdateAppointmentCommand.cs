using MediatR;
using AutoMapper;
using Application.Common.Models;
using Application.Features.Appointments.Contracts;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Application.Features.Appointments.Commands
{
    public record UpdateAppointmentCommand(UpdateAppointmentRequest Request) : IRequest<UpdateAppointmentResponse>;

    public class UpdateAppointmentCommandHandler : IRequestHandler<UpdateAppointmentCommand, UpdateAppointmentResponse>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public UpdateAppointmentCommandHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<UpdateAppointmentResponse> Handle(UpdateAppointmentCommand command, CancellationToken cancellationToken)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == command.Request.Id, cancellationToken);

            if (appointment == null)
            {
                return new UpdateAppointmentResponse 
                { 
                    Result = Result.Failure("Appointment not found.") 
                };
            }

            // Only the owner or an admin can update an appointment
            var isAdmin = _currentUserService.Claims.Any(c => c == "Admin");
            if (!isAdmin && appointment.UserId != _currentUserService.UserId)
            {
                return new UpdateAppointmentResponse 
                { 
                    Result = Result.Failure("You don't have permission to update this appointment.") 
                };
            }
            
            // Skip validations if appointment is being cancelled
            if (command.Request.IsCancelled == Domain.Enums.YesNo.Yes)
            {
                // Update the appointment - only fields related to cancellation
                appointment.Title = command.Request.Title;
                appointment.Description = command.Request.Description ?? string.Empty;
                appointment.IsConfirmed = command.Request.IsConfirmed;
                appointment.IsCancelled = command.Request.IsCancelled;
                appointment.CancellationReason = command.Request.CancellationReason;

                await _context.SaveChangesAsync(cancellationToken);

                return new UpdateAppointmentResponse
                {
                    Result = Result.Success()
                };
            }
            
            // Extract date parts for comparison
            var appointmentDate = command.Request.StartTime.Date;
            
            // Get all appointments for the user on the same day that are not cancelled, excluding the current one
            var userAppointmentsOnSameDay = await _context.Appointments
                .Where(a => a.UserId == appointment.UserId && 
                           a.StartTime.Date == appointmentDate &&
                           a.IsCancelled == Domain.Enums.YesNo.No &&
                           a.Id != command.Request.Id)
                .ToListAsync(cancellationToken);
            
            // Validation 1: Maximum 5 events per day
            if (userAppointmentsOnSameDay.Count >= 5)
            {
                return new UpdateAppointmentResponse
                {
                    Result = Result.Failure("No se pueden crear más de 5 eventos por día.")
                };
            }
            
            // Validation 2: Maximum 6 hours of events per day
            var newAppointmentDuration = (command.Request.EndTime - command.Request.StartTime).TotalHours;
            var existingAppointmentsTotalHours = userAppointmentsOnSameDay.Sum(a => (a.EndTime - a.StartTime).TotalHours);
            
            if (existingAppointmentsTotalHours + newAppointmentDuration > 6)
            {
                return new UpdateAppointmentResponse
                {
                    Result = Result.Failure("No se pueden superar las 6 horas de eventos por día.")
                };
            }
            
            // Validation 3: No time overlap with existing appointments
            bool hasOverlap = userAppointmentsOnSameDay.Any(a => 
                (command.Request.StartTime >= a.StartTime && command.Request.StartTime < a.EndTime) || // New appointment starts during existing appointment
                (command.Request.EndTime > a.StartTime && command.Request.EndTime <= a.EndTime) || // New appointment ends during existing appointment
                (command.Request.StartTime <= a.StartTime && command.Request.EndTime >= a.EndTime)); // New appointment completely encompasses existing appointment
            
            if (hasOverlap)
            {
                return new UpdateAppointmentResponse
                {
                    Result = Result.Failure("No puede haber superposición horaria con otros eventos.")
                };
            }

            // Update the appointment
            appointment.Title = command.Request.Title;
            appointment.Description = command.Request.Description;
            appointment.StartTime = command.Request.StartTime;
            appointment.EndTime = command.Request.EndTime;
            appointment.Location = command.Request.Location;
            appointment.IsConfirmed = command.Request.IsConfirmed;
            appointment.IsCancelled = command.Request.IsCancelled;
            appointment.CancellationReason = command.Request.CancellationReason;
            appointment.MeetingLink = command.Request.MeetingLink;
            appointment.SendReminder = command.Request.SendReminder;

            await _context.SaveChangesAsync(cancellationToken);

            return new UpdateAppointmentResponse
            {
                Result = Result.Success()
            };
        }
    }
} 