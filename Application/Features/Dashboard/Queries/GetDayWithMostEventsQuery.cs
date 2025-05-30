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
    public record GetDayWithMostEventsQuery(DateTime? StartDate = null, DateTime? EndDate = null, DateTime? WeekStart = null) : IRequest<DayWithMostEventsDto>;

    public class GetDayWithMostEventsQueryHandler : IRequestHandler<GetDayWithMostEventsQuery, DayWithMostEventsDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTime _dateTime;

        public GetDayWithMostEventsQueryHandler(
            IApplicationDbContext context,
            IDateTime dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        public async Task<DayWithMostEventsDto> Handle(GetDayWithMostEventsQuery request, CancellationToken cancellationToken)
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

        private DateTime StartOfWeek(DateTime dt)
        {
            int diff = (7 + (dt.DayOfWeek - DayOfWeek.Monday)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
} 