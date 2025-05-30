using Application.Common.Dtos.Email;
using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;
using Azure;
using EmailMessage = Azure.Communication.Email.EmailMessage;
using Azure.Communication.Email;
using Domain.Entities;

namespace Infrastructure.Services;
public class EmailService(IConfiguration configuration, IApplicationDbContext context, ICurrentUserService currentUserService) : IEmailService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Result> Send(string toAddresses, string subject, string body, bool isBodyHtml, bool? resentEmail, IEnumerable<AttachmentDto>? attachments, string? bcc = null)
    {
        string environmentName = _configuration.GetValue<string>("EnvironmentConfiguration:EnvironmentName") ?? string.Empty;
        bool productionMode = _configuration.GetValue<bool>("EnvironmentConfiguration:EmailProductionMode");
        string testAddresses = _configuration.GetValue<string>("EnvironmentConfiguration:EmailTestDestinataries");
        string displayName = "";
        string emailFrom = "";
        string errorMessage = "";

        if (!string.IsNullOrEmpty(environmentName))
        {
            subject += $" - ({environmentName} environment)";
        }

        try
        {
            #region Send email
            Result mailResponse;

            if (_configuration.GetValue<bool>("EmailConfiguration:IsSmtpService"))
            {
                emailFrom = _configuration.GetValue<string>("EmailConfiguration:SmtpFromAddress");
                displayName = _configuration.GetValue<string>("EmailConfiguration:SmtpFromName");

                mailResponse = await SendSMTP(
                    toAddresses,
                    subject,
                    body,
                    isBodyHtml,
                    resentEmail,
                    productionMode,
                    testAddresses,
                    displayName,
                    emailFrom,
                    attachments,
                    bcc
                );
            }
            else
            {
                displayName = _configuration.GetValue<string>("EmailConfiguration:AzureEmailDisplayName");
                emailFrom = _configuration.GetValue<string>("EmailConfiguration:AzureEmailFrom");

                mailResponse = await SendMailAzureService(
                    toAddresses,
                    subject,
                    body,
                    isBodyHtml,
                    resentEmail,
                    productionMode,
                    testAddresses,
                    displayName,
                    emailFrom,
                    attachments,
                    bcc
                    );
            }

            if (!mailResponse.Succeeded)
            {
                throw new Exception(mailResponse.Errors.FirstOrDefault() ?? "Email sending failed.");
            }
            #endregion

            return Result.Success();
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return Result.ExceptionFailure(ex);
        }
        finally
        {
            #region Email log 

            var mailLog = new EmailLog
            {
                ToAddresses = productionMode ? string.Join(";", toAddresses) : testAddresses,
                FromName = displayName,
                FromAddress = emailFrom,
                Subject = subject,
                Body = body,
                SentDate = DateTime.Now,
                UserId = string.IsNullOrEmpty(_currentUserService.UserId) ? null : _currentUserService.UserId,
                FullUserName = $"{_currentUserService.FirstName} {_currentUserService.LastName}",
                ErrorMessage = errorMessage,
                BccAddresses = bcc
            };

            await _context.SaveChangesAsync();
            _context.EmailLogs.Add(mailLog);

            #endregion
        }
    }

    private async Task<Result> SendSMTP(string toAddresses, string subject, string body, bool isBodyHtml, bool? resentEmail, bool productionMode, string testAdresses, string displayName, string emailFrom, IEnumerable<AttachmentDto>? attachments, string? bcc = null)
    {
        try
        {
            var smtpClient = new SmtpClient
            {
                Host = _configuration.GetValue<string>("EmailConfiguration:SmtpServer"),
                Port = _configuration.GetValue<int>("EmailConfiguration:SmtpPort"),
                EnableSsl = _configuration.GetValue<bool>("EmailConfiguration:EnableSsl"),
                UseDefaultCredentials = _configuration.GetValue<bool>("EmailConfiguration:UseDefaultCredentials"),
                Credentials = new NetworkCredential(
                        _configuration.GetValue<string>("EmailConfiguration:SmtpUsername"),
                        _configuration.GetValue<string>("EmailConfiguration:SmtpPassword"))
            };

            var fromAddress = new MailAddress(emailFrom, displayName);

            using (var msg = new MailMessage())
            {
                try
                {
                    msg.From = fromAddress;
                    msg.Sender = fromAddress;
                    if (productionMode)
                    {
                        msg.To.Add(toAddresses);
                    }
                    else
                    {
                        msg.To.Add(testAdresses);
                    }

                    if (!string.IsNullOrEmpty(bcc))
                    {
                        if(productionMode)
                        {
                            msg.Bcc.Add(bcc);
                        }
                        else
                        {
                            msg.Bcc.Add(testAdresses);
                        }
                    }

                    msg.Subject = subject;
                    msg.Body = body;
                    msg.IsBodyHtml = isBodyHtml;

                    if (attachments != null)
                    {
                        foreach (var attachment in attachments)
                        {
                            var ms = new MemoryStream();
                            await attachment.File.CopyToAsync(ms);
                            ms.Position = 0;
                            var attachmentData = new Attachment(ms, attachment.File.FileName);
                            if (!string.IsNullOrEmpty(attachment.ContentId))
                            {
                                attachmentData.ContentId = attachment.ContentId;
                            }
                            msg.Attachments.Add(attachmentData);
                        }
                    }

                    smtpClient.Send(msg);
                }
                catch (Exception Ex)
                {
                    throw Ex;
                }
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private async Task<Result> SendMailAzureService(string toAddresses, string subject, string body, bool isBodyHtml, bool? resentEmail, bool productionMode, string testAdresses, string displayName, string emailFrom, IEnumerable<AttachmentDto>? attachments, string? bcc = null)
    {
        try
        {
            string connectionString = _configuration["EmailConfiguration:AzureConnectionString"];
            EmailClient emailClient = new(connectionString);
            var emailContent = new EmailContent(subject);

            if (isBodyHtml)
            {
                emailContent.Html = body;
            }
            else
            {
                emailContent.PlainText = body;
            }

            var emailRecipients = new EmailRecipients(
                productionMode ?
                new List<EmailAddress>() { new EmailAddress(toAddresses) }
                 : new List<EmailAddress>() { new EmailAddress(testAdresses) }
                );

            if(!string.IsNullOrEmpty(bcc))
            {
                emailRecipients.BCC.Add(new EmailAddress(productionMode ? bcc : testAdresses));
            }

            var fromAddress = new MailAddress(emailFrom, displayName);

            var emailMessage = new EmailMessage(fromAddress.Address, emailRecipients, emailContent);

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    var ms = new MemoryStream();
                    await attachment.File.CopyToAsync(ms);
                    ms.Position = 0;
                    var emailAttachment = new EmailAttachment(attachment.File.FileName, "application/octet-stream", BinaryData.FromBytes(ms.ToArray()));
                    emailMessage.Attachments.Add(emailAttachment);
                }
            }

            var emailSendOperation = emailClient.Send(WaitUntil.Completed, emailMessage);


            return Result.Success();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}
