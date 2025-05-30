using MediatR;
using AutoMapper;
using Application.Common.Models;
using Domain.Entities;
using Application.Features.Appointments.Contracts;
using Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Application.Features.Appointments.Commands
{
    public record CreateAppointmentCommand(CreateAppointmentRequest Request) : IRequest<CreateAppointmentResponse>;

    public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, CreateAppointmentResponse>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public CreateAppointmentCommandHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<CreateAppointmentResponse> Handle(CreateAppointmentCommand command, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId ?? throw new Exception("User ID is required");
            
            // Extract date parts for comparison
            var appointmentDate = command.Request.StartTime.Date;
            
            // Get all appointments for the user on the same day that are not cancelled
            var userAppointmentsOnSameDay = await _context.Appointments
                .Where(a => a.UserId == userId && 
                           a.StartTime.Date == appointmentDate &&
                           a.IsCancelled == Domain.Enums.YesNo.No)
                .ToListAsync(cancellationToken);
            
            // Validation 1: Maximum 5 events per day
            if (userAppointmentsOnSameDay.Count >= 5)
            {
                return new CreateAppointmentResponse
                {
                    Id = 0,
                    Result = Result.Failure("No se pueden crear más de 5 eventos por día.")
                };
            }
            
            // Validation 2: Maximum 6 hours of events per day
            var newAppointmentDuration = (command.Request.EndTime - command.Request.StartTime).TotalHours;
            var existingAppointmentsTotalHours = userAppointmentsOnSameDay.Sum(a => (a.EndTime - a.StartTime).TotalHours);
            
            if (existingAppointmentsTotalHours + newAppointmentDuration > 6)
            {
                return new CreateAppointmentResponse
                {
                    Id = 0,
                    Result = Result.Failure("No se pueden superar las 6 horas de eventos por día.")
                };
            }
            
            // Validation 3: No time overlap with existing appointments, TODO: Validate if this is working
/*             bool hasOverlap = userAppointmentsOnSameDay.Any(a => 
                (command.Request.StartTime >= a.StartTime && command.Request.StartTime < a.EndTime) || // New appointment starts during existing appointment
                (command.Request.EndTime > a.StartTime && command.Request.EndTime <= a.EndTime) || // New appointment ends during existing appointment
                (command.Request.StartTime <= a.StartTime && command.Request.EndTime >= a.EndTime)); // New appointment completely encompasses existing appointment
            
            if (hasOverlap)
            {
                return new CreateAppointmentResponse
                {
                    Id = 0,
                    Result = Result.Failure("No puede haber superposición horaria con otros eventos.")
                };
            } */

            var appointment = new Appointment
            {
                Title = command.Request.Title,
                Description = command.Request.Description ?? string.Empty,
                StartTime = command.Request.StartTime.Date,
                EndTime = command.Request.EndTime.Date,
                Location = command.Request.Location,
                UserId = userId,
                IsConfirmed = command.Request.IsConfirmed,
                IsCancelled = Domain.Enums.YesNo.No,
                MeetingLink = command.Request.MeetingLink,
                SendReminder = command.Request.SendReminder
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync(cancellationToken);

            return new CreateAppointmentResponse
            {
                Id = appointment.Id,
                Result = Result.Success()
            };
        }
    }
} 