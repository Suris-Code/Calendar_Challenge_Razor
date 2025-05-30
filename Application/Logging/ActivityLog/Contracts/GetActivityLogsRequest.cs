using Application.Common.Models;

namespace Application.Logging.ActivityLog.Contracts;
public class GetActivityLogsRequest : PaginatedWithFilter<GetActivityLogsFilters>
{
}

public class GetActivityLogsFilters
{
    public DatetimeRange? ActivityDate { get; set; }
    public string? Email { get; set; }
    public string? Controller { get; set; }
}
