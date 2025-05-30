using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces;
using Application.Common.Models;

namespace Application.Features.Dashboard.Queries
{
    public record GetWeeklyEventsTotalQuery(DateTime? StartDate = null, DateTime? EndDate = null, DateTime? WeekStart = null) : IRequest<int>;

    public class GetWeeklyEventsTotalQueryHandler : IRequestHandler<GetWeeklyEventsTotalQuery, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTime _dateTime;

        public GetWeeklyEventsTotalQueryHandler(
            IApplicationDbContext context,
            IDateTime dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        public async Task<int> Handle(GetWeeklyEventsTotalQuery request, CancellationToken cancellationToken)
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

            return await _context.Appointments
                .Where(a => a.StartTime >= startDate && a.StartTime < endDate)
                .CountAsync(cancellationToken);
        }

        private DateTime StartOfWeek(DateTime dt)
        {
            int diff = (7 + (dt.DayOfWeek - DayOfWeek.Monday)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
} 