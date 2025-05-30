using Application.Common.Models;
using Application.Logging.UserActivityLog.Dtos;

namespace Application.Logging.UserActivityLog.Contracts
{
    public class GetUserActivityLogsResponse : PaginatedResponse<IEnumerable<UserActivityLogDto>>
    {
    }
}