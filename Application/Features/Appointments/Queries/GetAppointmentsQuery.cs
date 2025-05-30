using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Application.Common.Models;
using Microsoft.AspNetCore.Identity;
using Domain.Entities;
using Application.Features.Appointments.Contracts;
using Application.Features.Appointments.Dtos;
using Application.Common.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;

namespace Application.Features.Appointments.Queries
{
    public record GetAppointmentsQuery(GetAppointmentsRequest Request) : IRequest<GetAppointmentsResponse>;

    public class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, GetAppointmentsResponse>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTime _dateTime;

        public GetAppointmentsQueryHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IDateTime dateTime)
        {
            _context = context;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _dateTime = dateTime;
        }

        public async Task<GetAppointmentsResponse> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Appointments
                .Include(a => a.User)
                .AsQueryable();

            var isAdmin = _currentUserService.Claims.Any(c => c == "Admin");

            // Apply security filter - users can only see their own appointments unless they are admins
            if (!isAdmin)
            {
                query = query.Where(a => a.UserId == _currentUserService.UserId);
            }

            if (request.Request.Filters != null)
            {
                query = ApplyFilters(query, request.Request.Filters);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var appointments = await query
                .OrderBy(x => x.StartTime)
                .ToListAsync(cancellationToken);

            var appointmentDtos = _mapper.Map<AppointmentDto[]>(appointments);

            return new GetAppointmentsResponse()
            {
                Data = appointmentDtos,
                Result = Result.Success(),
                TotalCount = totalCount
            };
        }

        private IQueryable<Appointment> ApplyFilters(IQueryable<Appointment> query, GetAppointmentsFilters filters)
        {
            // Filter by date range if specified
            if (filters.AppointmentDateRange?.From != null)
            {
                query = query.Where(a => a.StartTime >= filters.AppointmentDateRange.From);
            }
            if (filters.AppointmentDateRange?.To != null)
            {
                query = query.Where(a => a.StartTime <= filters.AppointmentDateRange.To);
            }

            // Filter by month and year
            if (filters.Year.HasValue && filters.Month.HasValue)
            {
                var startOfMonth = new DateTime(filters.Year.Value, filters.Month.Value, 1);
                var endOfMonth = startOfMonth.AddMonths(1);
                query = query.Where(a => a.StartTime >= startOfMonth && a.StartTime < endOfMonth);
            }

            // Filter by week start
            if (filters.WeekStart.HasValue)
            {
                var endOfWeek = filters.WeekStart.Value.AddDays(7);
                query = query.Where(a => a.StartTime >= filters.WeekStart.Value && a.StartTime < endOfWeek);
            }

            // Filter by confirmation status
            if (filters.ShowOnlyConfirmed.HasValue && filters.ShowOnlyConfirmed.Value)
            {
                query = query.Where(a => a.IsConfirmed == YesNo.Yes);
            }

            // Filter by cancellation status
            if (filters.ShowOnlyCancelled.HasValue && filters.ShowOnlyCancelled.Value)
            {
                query = query.Where(a => a.IsCancelled == YesNo.Yes);
            }

            return query;
        }
    }
} 