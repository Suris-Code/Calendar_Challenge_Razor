using Application.Common.Mappings;
using Domain.Entities;
using Domain.Enums;

namespace Application.Logging.ErrorLogs.Dtos;

public class ErrorLogDto : IMapFrom<Domain.Entities.ErrorLog>
{
    public long Id { get; set; }
    public ErrorLogType Type { get; set; }

    public string TypeDescription => Type.Description();
    public DateTime Date { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FullUserName { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string QueryString { get; set; } = string.Empty;
    public required string Message { get; set; } 
    public string MemberName { get; set; } = string.Empty;
    public string SourceFilePath { get; set; } = string.Empty;
    public int? SourceLineNumber { get; set; }
    public int? SourceColumnNumber { get; set; }
    public int? HttpErrorCode { get; set; }
}
