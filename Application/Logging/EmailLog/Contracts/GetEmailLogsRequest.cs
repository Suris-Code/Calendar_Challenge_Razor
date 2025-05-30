using Application.Common.Models;

public class GetEmailLogsRequest : PaginatedWithFilter<GetEmailLogsFilters>
{
} 

public class GetEmailLogsFilters
{
    public string? ToEmail { get; set; }
    public DatetimeRange? SentDate { get; set; }
    public string? Subject { get; set; }
}
