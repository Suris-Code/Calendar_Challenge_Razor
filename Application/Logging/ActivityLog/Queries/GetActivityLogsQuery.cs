using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Application.Common.Models;
using Microsoft.AspNetCore.Identity;
using Domain.Entities;
using Application.Logging.ActivityLog.Contracts;
using Application.Logging.ActivityLog.Dtos;
using Application.Common.Interfaces;

namespace Application.Logging.ActivityLog.Queries
{
    public record GetActivityLogsQuery(GetActivityLogsRequest Request) : IRequest<GetActivityLogsResponse>;

    public class GetActivityLogsQueryHandler : IRequestHandler<GetActivityLogsQuery, GetActivityLogsResponse>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetActivityLogsQueryHandler(
            IApplicationDbContext context,
            IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<GetActivityLogsResponse> Handle(GetActivityLogsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ActivityLogs
                .Include(log => log.User)
                .AsQueryable();

            if (request.Request.Filters != null)
            {
                query = ApplyFilters(query, request.Request.Filters);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var logs = await query
                .OrderByDescending(x => x.Date)
                .Skip(request.Request.Page * request.Request.PageSize)
                .Take(request.Request.PageSize)
                .ToListAsync(cancellationToken);

            var logDtos = _mapper.Map<ActivityLogDto[]>(logs);

            return new GetActivityLogsResponse()
            {
                Data = logDtos,
                Result = Result.Success(),
                TotalCount = totalCount
            };
        }

        private IQueryable<Domain.Entities.ActivityLog> ApplyFilters(IQueryable<Domain.Entities.ActivityLog> query, GetActivityLogsFilters filters)
        {
            if (filters.Email != null)
            {
                query = query.Where(log => log.FullUserName != null && log.FullUserName.Contains(filters.Email));
            }
            if (filters.Controller != null)
            {
                query = query.Where(log => log.ControllerName != null && log.ControllerName.Contains(filters.Controller));
            }
            if (filters.ActivityDate?.From != null)
            {
                query = query.Where(log => log.Date >= filters.ActivityDate.From);
            }
            if (filters.ActivityDate?.To != null)
            {
                query = query.Where(log => log.Date <= filters.ActivityDate.To);
            }
            return query;
        }
    }
}