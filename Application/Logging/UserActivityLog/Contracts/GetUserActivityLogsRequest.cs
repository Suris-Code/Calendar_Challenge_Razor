using Application.Common.Models;

namespace Application.Logging.UserActivityLog.Contracts
{
    public class GetUserActivityLogsRequest : PaginatedWithFilter<GetUserActivityLogsFilters>
    {
    }

    public class GetUserActivityLogsFilters
    {
        public string? Email { get; set; }
        public string? Action { get; set; }
        public DatetimeRange? ActivityDate { get; set; }
        public string? IpAddress { get; set; }
    }
}