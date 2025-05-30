using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.AspNetCore.Identity;
using Domain.Entities;
using Application.Logging.UserActivityLog.Contracts;
using Application.Logging.UserActivityLog.Dtos;

namespace Application.Logging.UserActivityLog.Queries
{
    public record GetUserActivityLogsQuery(GetUserActivityLogsRequest Request) : IRequest<GetUserActivityLogsResponse>;

    public class GetUserActivityLogsQueryHandler : IRequestHandler<GetUserActivityLogsQuery, GetUserActivityLogsResponse>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IErrorLogService _errorLogService;
        public GetUserActivityLogsQueryHandler(IApplicationDbContext context, IMapper mapper, UserManager<ApplicationUser> userManager, ICurrentUserService currentUserService, IErrorLogService errorLogService)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
            _currentUserService = currentUserService;
            _errorLogService = errorLogService;
        }

        public async Task<GetUserActivityLogsResponse> Handle(GetUserActivityLogsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var logsSet = _context.UserActivityLogs.AsQueryable();

                if (request.Request.Filters != null)
                {
                    logsSet = ApplyFilters(logsSet, request.Request.Filters);
                }

                var totalCount = await logsSet.CountAsync(cancellationToken);

                var logs = await logsSet
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip(request.Request.Page * request.Request.PageSize)
                    .Take(request.Request.PageSize)
                    .ToListAsync(cancellationToken);

                var logDtos = _mapper.Map<UserActivityLogDto[]>(logs);

                return new GetUserActivityLogsResponse()
                {
                    Data = logDtos,
                    TotalCount = totalCount,
                    Result = Result.Success()
                };
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(
         ex,
         _currentUserService.UserId,
         _currentUserService.UserName,
         _currentUserService.IpAddress);
                throw;
            }
        }

        private IQueryable<Domain.Entities.UserActivityLog> ApplyFilters(IQueryable<Domain.Entities.UserActivityLog> query, GetUserActivityLogsFilters filters)
        {
            if (filters.Email != null)
            {
                query = query.Where(log => log.UserName.Contains(filters.Email));
            }
            if (filters.Action != null)
            {
                query = query.Where(log => log.Action.Contains(filters.Action));
            }
            if (filters.ActivityDate?.From != null)
            {
                query = query.Where(log => log.CreatedAt >= filters.ActivityDate.From);
            }
            if (filters.ActivityDate?.To != null)
            {
                query = query.Where(log => log.CreatedAt <= filters.ActivityDate.To);
            }
            if (filters.IpAddress != null)
            {
                query = query.Where(log => log.IpAddress.Contains(filters.IpAddress));
            }
            return query;
        }
    }
}