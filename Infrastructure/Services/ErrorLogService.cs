using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Services
{
    public class ErrorLogService : IErrorLogService
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTime _dateTime;

        public ErrorLogService(IApplicationDbContext context, IDateTime dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        public async Task LogErrorAsync(
            Exception ex,
            string userId,
            string fullUserName,
            string ipAddress,
            string? path = null,
            string? queryString = null,
            int? httpErrorCode = null,
            CancellationToken cancellationToken = default)
        {
            var log = new ErrorLog
            {
                Date = _dateTime.Now,
                Type = ErrorLogType.Exception,
                UserId = userId,
                FullUserName = fullUserName,
                RemoteIPAddress = ipAddress,
                Path = path,
                QueryString = queryString,
                Message = ex.Message,
                MemberName = ex.TargetSite?.Name,
                SourceFilePath = ex.StackTrace,
                HttpErrorCode = httpErrorCode
            };

            await _context.ErrorLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
} 