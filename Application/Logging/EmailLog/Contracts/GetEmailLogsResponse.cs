using Application.Common.Models;
using Application.Logging.EmailLog.Dtos;

namespace Application.Logging.EmailLog.Contracts
{
    public class GetEmailLogsResponse : PaginatedResponse<IEnumerable<EmailLogDto>>
    {
    }
}