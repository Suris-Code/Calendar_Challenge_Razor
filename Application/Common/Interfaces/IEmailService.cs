using Application.Common.Dtos.Email;

namespace Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task<Models.Result> Send(string toAddresses, string subject, string body, bool isBodyHtml, bool? resentEmail, IEnumerable<AttachmentDto>? attachments, string? bcc = null);
    }
}
