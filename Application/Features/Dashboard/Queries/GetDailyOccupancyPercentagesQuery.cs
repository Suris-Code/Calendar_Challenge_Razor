using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces;
using Application.Features.Dashboard.Dtos;

namespace Application.Features.Dashboard.Queries
{
    public record GetDailyOccupancyPercentagesQuery(DateTime? StartDate = null, DateTime? EndDate = null, DateTime? WeekStart = null) : IRequest<DailyOccupancyPercentageDto[]>;

    public class GetDailyOccupancyPercentagesQueryHandler : IRequestHandler<GetDailyOccupancyPercentagesQuery, DailyOccupancyPercentageDto[]>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTime _dateTime;

        public GetDailyOccupancyPercentagesQueryHandler(
            IApplicationDbContext context,
            IDateTime dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        public async Task<DailyOccupancyPercentageDto[]> Handle(GetDailyOccupancyPercentagesQuery request, CancellationToken cancellationToken)
        {
            // Get date range or use current week if not specified
            DateTime startDate, endDate;
            
            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                startDate = request.StartDate.Value;
                endDate = request.EndDate.Value;
            }
            else
            {
                startDate = request.WeekStart ?? StartOfWeek(_dateTime.Now);
                endDate = startDate.AddDays(7);
            }

            const double maxWorkingHours = 6.0; // Maximum working hours per day (0-6 hours)
            var dailyOccupancy = new List<DailyOccupancyPercentageDto>();

            // Fetch all appointments for the week in one query to reduce database calls
            var weekAppointments = await _context.Appointments
                .Where(a => a.StartTime >= startDate && a.StartTime < endDate)
                .ToListAsync(cancellationToken);

            for (var date = startDate; date < endDate; date = date.AddDays(1))
            {
                // Filter and calculate in memory
                var totalHoursForDay = weekAppointments
                    .Where(a => a.StartTime.Date == date.Date)
                    .Sum(a => (a.EndTime - a.StartTime).TotalHours);

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