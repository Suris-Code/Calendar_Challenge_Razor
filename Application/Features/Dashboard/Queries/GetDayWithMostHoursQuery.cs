using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces;
using Application.Features.Dashboard.Dtos;

namespace Application.Features.Dashboard.Queries
{
    public record GetDayWithMostHoursQuery(DateTime? StartDate = null, DateTime? EndDate = null, DateTime? WeekStart = null) : IRequest<DayWithMostHoursDto>;

    public class GetDayWithMostHoursQueryHandler : IRequestHandler<GetDayWithMostHoursQuery, DayWithMostHoursDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTime _dateTime;

        public GetDayWithMostHoursQueryHandler(
            IApplicationDbContext context,
            IDateTime dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        public async Task<DayWithMostHoursDto> Handle(GetDayWithMostHoursQuery request, CancellationToken cancellationToken)
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

            // First, fetch the appointments for the week
            var appointments = await _context.Appointments
                .Where(a => a.StartTime >= startDate && a.StartTime < endDate)
                .ToListAsync(cancellationToken);

            // Then perform the grouping and calculation in memory
            var result = appointments
                .GroupBy(a => a.StartTime.Date)
                .Select(g => new DayWithMostHoursDto
                {
                    Date = g.Key,
                    TotalHours = g.Sum(a => (a.EndTime - a.StartTime).TotalHours)
                })
                .OrderByDescending(d => d.TotalHours)
                .FirstOrDefault();

            return result ?? new DayWithMostHoursDto { Date = startDate, TotalHours = 0 };
        }

        private DateTime StartOfWeek(DateTime dt)
        {
            int diff = (7 + (dt.DayOfWeek - DayOfWeek.Monday)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
} 