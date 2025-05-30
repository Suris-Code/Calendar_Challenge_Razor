using Microsoft.AspNetCore.Http;

namespace Application.Common.Dtos.Email;

public class AttachmentDto
{
    public IFormFile File { get; set; }
    public string ContentId { get; set; } = string.Empty;

    public static AttachmentDto Create(IFormFile file, string contentId = "")
    {
        return new AttachmentDto { File = file, ContentId = contentId };
    }

    public static AttachmentDto CreateFromBytes(byte[] bytes, string fileName, string contentId = "")
    {
        return new AttachmentDto { File = new FormFile(new MemoryStream(bytes), 0, bytes.Length, fileName, fileName), ContentId = contentId };
    }

    public static AttachmentDto CreateFromStream(Stream stream, string fileName, string contentId = "")
    {
        return new AttachmentDto { File = new FormFile(stream, 0, stream.Length, fileName, fileName), ContentId = contentId };
    }

    public static AttachmentDto CreateFromBase64(string base64, string fileName, string contentId = "")
    {
        byte[] bytes = Convert.FromBase64String(base64);
        return CreateFromBytes(bytes, fileName, contentId);
    }

    public static AttachmentDto CreateFromPath(string path, string fileName, string contentId = "")
    {
        if (!System.IO.File.Exists(path))
        {
            throw new Exception($"File '{path}' not found.");
        }

        byte[] bytes = System.IO.File.ReadAllBytes(path);
        return CreateFromBytes(bytes, fileName, contentId);
    }
}

