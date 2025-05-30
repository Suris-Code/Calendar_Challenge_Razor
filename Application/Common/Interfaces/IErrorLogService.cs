namespace Application.Common.Interfaces
{
    public interface IErrorLogService
    {
        Task LogErrorAsync(
            Exception ex,
            string userId,
            string fullUserName,
            string ipAddress,
            string? path = null,
            string? queryString = null,
            int? httpErrorCode = null,
            CancellationToken cancellationToken = default);
    }
} 