namespace Application.Common.Interfaces;
public interface IUserActivityLogService
{
    Task LogUserActivityAsync(string action, string description, CancellationToken cancellationToken = default);
} 