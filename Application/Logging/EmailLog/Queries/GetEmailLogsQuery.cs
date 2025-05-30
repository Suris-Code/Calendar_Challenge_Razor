using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.AspNetCore.Identity;
using Domain.Entities;
using Application.Logging.EmailLog.Contracts;
using Application.Logging.EmailLog.Dtos;

public record GetEmailLogsQuery(GetEmailLogsRequest Request) : IRequest<GetEmailLogsResponse>;

public class GetEmailLogsQueryHandler : IRequestHandler<GetEmailLogsQuery, GetEmailLogsResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IErrorLogService _errorLogService;
    public GetEmailLogsQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUserService currentUserService, IErrorLogService errorLogService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _errorLogService = errorLogService;
    }

    public async Task<GetEmailLogsResponse> Handle(GetEmailLogsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.EmailLogs.AsQueryable();

            if (request.Request.Filters != null)
            {
                query = ApplyFilters(query, request.Request.Filters);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var emailLogs = await query
                .OrderByDescending(x => x.SentDate)
                .Skip(request.Request.Page * request.Request.PageSize)
                .Take(request.Request.PageSize)
                .ToListAsync(cancellationToken);

            var emailLogDtos = _mapper.Map<EmailLogDto[]>(emailLogs);

            return new GetEmailLogsResponse()
            {
                Data = emailLogDtos,
                Result = Result.Success(),
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

    private IQueryable<Domain.Entities.EmailLog> ApplyFilters(IQueryable<Domain.Entities.EmailLog> query, GetEmailLogsFilters filters)
    {
        if (filters.ToEmail != null)
            query = query.Where(x => x.ToAddresses.Contains(filters.ToEmail));

        if (filters.SentDate?.From != null)
            query = query.Where(x => x.SentDate >= filters.SentDate.From);

        if (filters.SentDate?.To != null)
            query = query.Where(x => x.SentDate <= filters.SentDate.To);

        if (filters.Subject != null)
            query = query.Where(x => x.Subject.Contains(filters.Subject));

        return query;
    }
} 