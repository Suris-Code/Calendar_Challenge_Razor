using Domain.Enums;

namespace Application.Logging.ErrorLogs.Contracts;

public class CreateErrorLogRequest
{
    public required string Message { get; set; }
    public ErrorLogType? Type { get; set; }
    public int? HttpErrorCode { get; set; }
    public string? Path { get; set; }
    public string? QueryString { get; set; }
    public string? MemberName { get; set; }
    public string? SourceFilePath { get; set; }
    public int? SourceLineNumber { get; set; }
    public int? SourceColumnNumber { get; set; }
}
