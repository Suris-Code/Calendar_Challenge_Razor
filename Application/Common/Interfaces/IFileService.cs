using Microsoft.AspNetCore.Http;

namespace Application.Common.Interfaces;

public interface IFileService
{
    Task<byte[]> GetFileAsync(string path, CancellationToken cancellationToken);
    Task SaveFileAsync(string path, IFormFile formFile, CancellationToken cancellationToken);
    Task<string[]> GetFilesAsync(string directoryPath, CancellationToken cancellationToken);
    Task DeleteFileAsync(string path, CancellationToken cancellationToken);
}
