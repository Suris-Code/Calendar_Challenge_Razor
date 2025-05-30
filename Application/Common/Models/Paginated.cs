namespace Application.Common.Models;

public abstract class Paginated
{
    public int Page { get; set; } = 0;
    public int PageSize { get; set; } = 10;
}

public class PaginatedWithFilter<T> : Paginated
{
    public T? Filters { get; set; }
}

public class DatetimeRange
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}