using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class FileService : IFileService
{
    private readonly IConfiguration _configuration;
    public FileService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private void CreateDirectory(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    public async Task<byte[]> GetFileAsync(string path, CancellationToken cancellationToken)
    {
        string fullPath = Path.Combine(
            _configuration.GetValue<string>("EnvironmentConfiguration:FileStoragePath")
                ?? throw new InvalidOperationException("FileStoragePath configuration is missing"),
            path);


        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException();
        }

        return await File.ReadAllBytesAsync(fullPath, cancellationToken);
    }

    public async Task SaveFileAsync(string path, IFormFile formFile, CancellationToken cancellationToken)
    {
        string fullPath = Path.Combine(
            _configuration.GetValue<string>("EnvironmentConfiguration:FileStoragePath")
                ?? throw new InvalidOperationException("FileStoragePath configuration is missing"),
            path);

        CreateDirectory(fullPath);

        fullPath = Path.Combine(fullPath, formFile.FileName);
        using var stream = new FileStream(fullPath, FileMode.Create);
        await formFile.CopyToAsync(stream, cancellationToken);
    }
    public async Task<string[]> GetFilesAsync(string directoryPath, CancellationToken cancellationToken)
    {
        string fullPath = Path.Combine(
            _configuration.GetValue<string>("EnvironmentConfiguration:FileStoragePath")
                ?? throw new InvalidOperationException("FileStoragePath configuration is missing"),
            directoryPath);

        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException("The specified directory does not exist.");
        }

        return await Task.Run(() => Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories), cancellationToken);
    }

    public async Task DeleteFileAsync(string path, CancellationToken cancellationToken)
    {
        string fullPath = Path.Combine(
            _configuration.GetValue<string>("EnvironmentConfiguration:FileStoragePath")
                ?? throw new InvalidOperationException("FileStoragePath configuration is missing"),
            path);

        await Task.Run(() => File.Delete(fullPath), cancellationToken);
    }
}