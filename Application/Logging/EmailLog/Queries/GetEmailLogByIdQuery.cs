using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Application.Common.Models;
using Application.Logging.EmailLog.Contracts;
using Application.Logging.EmailLog.Dtos;

namespace Application.Logging.EmailLog.Queries
{
    public record GetEmailLogByIdQuery(GetEmailLogByIdRequest Request) : IRequest<GetEmailLogByIdResponse>;

    public class GetEmailLogByIdQueryHandler : IRequestHandler<GetEmailLogByIdQuery, GetEmailLogByIdResponse>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IErrorLogService _errorLogService;
        public GetEmailLogByIdQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUserService currentUserService, IErrorLogService errorLogService)
        {
            _context = context;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _errorLogService = errorLogService;
        }

        public async Task<GetEmailLogByIdResponse> Handle(GetEmailLogByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var emailLog = await _context.EmailLogs
               .FirstOrDefaultAsync(x => x.Id == request.Request.Id, cancellationToken);

                if (emailLog == null)
                    return new GetEmailLogByIdResponse
                    {
                        Result = Result.Failure($"Email log with id {request.Request.Id} not found.")
                    };

                return new GetEmailLogByIdResponse
                {
                    Data = _mapper.Map<EmailLogDto>(emailLog),
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
    }
}