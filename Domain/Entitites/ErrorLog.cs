using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class ErrorLog
    {
        public long Id { get; set; }
        public ErrorLogType? Type { get; set; }
        public DateTime Date { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullUserName { get; set; } = string.Empty;
        public string RemoteIPAddress { get; set; } = string.Empty;
        public string RemotePort { get; set; } = string.Empty;
        public string LocalIPAddress { get; set; } = string.Empty;
        public string LocalPort { get; set; } = string.Empty;
        public string? Path { get; set; }
        public string? QueryString { get; set; }
        public string? Message { get; set; }
        public string? MemberName { get; set; }
        public string? SourceFilePath { get; set; }
        public int? SourceLineNumber { get; set; }
        public int? SourceColumnNumber { get; set; }
        public int? HttpErrorCode { get; set; }
    }
}
