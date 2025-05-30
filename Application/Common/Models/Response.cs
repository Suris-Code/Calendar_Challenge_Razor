namespace Application.Common.Models;

public abstract class Response
{
    public required Result Result { get; set; }
}
public abstract class Response<T> : Response
{
    public T? Data { get; set; }
}

public class PaginatedResponse<T> : Response<T>
{
    public int TotalCount { get; set; }
}
