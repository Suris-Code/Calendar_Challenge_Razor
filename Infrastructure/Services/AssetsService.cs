using Application.Common.Interfaces;

namespace Infrastructure.Services;

public class AssetsService : IAssetsService
{
    public string GetAssetPath(string filePath) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", filePath);
    public string GetAssetTextContent(string filePath) => File.ReadAllText(GetAssetPath(filePath));
}