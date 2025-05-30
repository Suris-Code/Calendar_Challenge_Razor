using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Dashboard.Contracts;
using Application.Features.Dashboard.Dtos;
using Domain.Entities;

namespace Application.Features.Dashboard.Queries
{
    public record GetDashboardStatisticsQuery(GetDashboardStatisticsRequest Request) : IRequest<GetDashboardStatisticsResponse>;

    public class GetDashboardStatisticsQueryHandler : IRequestHandler<GetDashboardStatisticsQuery, GetDashboardStatisticsResponse>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDateTime _dateTime;

        public GetDashboardStatisticsQueryHandler(
            IApplicationDbContext context,
            IMapper mapper,
            IDateTime dateTime)
        {
            _context = context;
            _mapper = mapper;
            _dateTime = dateTime;
        }

        public async Task<GetDashboardStatisticsResponse> Handle(GetDashboardStatisticsQuery request, CancellationToken cancellationToken)
        {
            // Get date range or use current week if not specified
            DateTime startDate, endDate;
            
            if (request.Request.StartDate.HasValue && request.Request.EndDate.HasValue)
            {
                startDate = request.Request.StartDate.Value;
                endDate = request.Request.EndDate.Value;
            }
            else
            {
                startDate = request.Request.WeekStart ?? StartOfWeek(_dateTime.Now);
                endDate = startDate.AddDays(7);
            }

            var dashboardDto = new DashboardDto
            {
                TotalWeeklyEvents = await GetTotalWeeklyEvents(startDate, endDate, cancellationToken),
                DayWithMostEvents = await GetDayWithMostEvents(startDate, endDate, cancellationToken),
                DayWithMostHours = await GetDayWithMostHours(startDate, endDate, cancellationToken),
                DailyOccupancyPercentages = await GetDailyOccupancyPercentages(startDate, endDate, cancellationToken)
            };

            return new GetDashboardStatisticsResponse
            {
                Data = dashboardDto,
                Result = Result.Success()
            };
        }

        private async Task<int> GetTotalWeeklyEvents(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            return await _context.Appointments
                .Where(a => a.StartTime >= startDate && a.StartTime < endDate)
                .CountAsync(cancellationToken);
        }

        private async Task<DayWithMostEventsDto> GetDayWithMostEvents(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            var result = await _context.Appointments
                .Where(a => a.StartTime >= startDate && a.StartTime < endDate)
                .GroupBy(a => a.StartTime.Date)
                .Select(g => new DayWithMostEventsDto
                {
                    Date = g.Key,
                    EventCount = g.Count()
                })
                .OrderByDescending(d => d.EventCount)
                .FirstOrDefaultAsync(cancellationToken);

            return result ?? new DayWithMostEventsDto { Date = startDate, EventCount = 0 };
        }

        private async Task<DayWithMostHoursDto> GetDayWithMostHours(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            var result = await _context.Appointments
                .Where(a => a.StartTime >= startDate && a.StartTime < endDate)
                .GroupBy(a => a.StartTime.Date)
                .Select(g => new DayWithMostHoursDto
                {
                    Date = g.Key,
                    TotalHours = g.Sum(a => (a.EndTime - a.StartTime).TotalHours)
                })
                .OrderByDescending(d => d.TotalHours)
                .FirstOrDefaultAsync(cancellationToken);

            return result ?? new DayWithMostHoursDto { Date = startDate, TotalHours = 0 };
        }

        private async Task<DailyOccupancyPercentageDto[]> GetDailyOccupancyPercentages(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            const double maxWorkingHours = 6.0; // Maximum working hours per day (0-6 hours)
            var dailyOccupancy = new List<DailyOccupancyPercentageDto>();

            for (var date = startDate; date < endDate; date = date.AddDays(1))
            {
                var totalHoursForDay = await _context.Appointments
                    .Where(a => a.StartTime.Date == date.Date)
                    .SumAsync(a => (a.EndTime - a.StartTime).TotalHours, cancellationToken);

                var occupancyPercentage = Math.Min(100, (totalHoursForDay / maxWorkingHours) * 100);

                dailyOccupancy.Add(new DailyOccupancyPercentageDto
                {
                    Date = date,
                    OccupancyPercentage = occupancyPercentage
                });
            }

            return dailyOccupancy.ToArray();
        }

        private DateTime StartOfWeek(DateTime dt)
        {
            int diff = (7 + (dt.DayOfWeek - DayOfWeek.Monday)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
} 