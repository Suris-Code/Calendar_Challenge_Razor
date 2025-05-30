namespace Application.Common.Interfaces;

public interface IAssetsService
{
    string GetAssetPath(string filePath);
    string GetAssetTextContent(string filePath);
}