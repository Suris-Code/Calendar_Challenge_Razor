using Application.Common.Models;
using Application.Logging.ActivityLog.Dtos;

namespace Application.Logging.ActivityLog.Contracts
{
    public class GetActivityLogsResponse : PaginatedResponse<IEnumerable<ActivityLogDto>>
    {
    }
}