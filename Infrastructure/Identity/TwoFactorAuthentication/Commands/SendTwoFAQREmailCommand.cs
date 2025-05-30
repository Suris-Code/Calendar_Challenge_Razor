using System.Text.RegularExpressions;
using Application.Common.Dtos.Email;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using Google.Authenticator;
using Infrastructure.Identity.TwoFactorAuthentication.Commands;
using Infrastructure.Identity.TwoFactorAuthentication.Contracts;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

public class SendTwoFAQREmailCommand(SendTwoFAQREmailRequest request) : IRequest<SendTwoFAQREmailResponse>
{
    readonly SendTwoFAQREmailRequest _request = request;

    public SendTwoFAQREmailRequest Request => _request;
}

public class SendTwoFAQREmailCommandHandler(IApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager, ISender mediator, IWebHostEnvironment webHostEnvironment, ICrypto crypto, IConfiguration configuration, TwoFactorAuthenticator tfa, IEmailService emailService, IAssetsService assetsService) : IRequestHandler<SendTwoFAQREmailCommand, SendTwoFAQREmailResponse>
{
    private readonly IApplicationDbContext _applicationDbContext = applicationDbContext;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ISender _mediator = mediator;
    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
    private readonly ICrypto _crypto = crypto;
    private readonly IConfiguration _configuration = configuration;
    private readonly TwoFactorAuthenticator _tfa = tfa;
    private readonly IEmailService _emailService = emailService;
    private readonly IAssetsService _assetsService = assetsService;

    public async Task<SendTwoFAQREmailResponse> Handle(SendTwoFAQREmailCommand command, CancellationToken cancellationToken)
    {
        try
        {
            string _barCodeImage;
            string _manualCode;
            string _currentToken;

            if (string.IsNullOrWhiteSpace(command.Request.UserId))
            {
                return new SendTwoFAQREmailResponse
                {
                    Result = Result.Failure("User not found.")
                };
            }

            var user = await _userManager.FindByEmailAsync(command.Request.UserId);
            if (user is null)
            {
                return new SendTwoFAQREmailResponse
                {
                    Result = Result.Failure("Invalid user.")
                };
            }

            #region If no privateCode2FA, generate it
            if (string.IsNullOrEmpty(user.PrivateCode2FA))
            {
                var response = await _mediator.Send(new ConfigureNewQRAppCommand(new ConfigureNewQRAppRequest { UserId = user.Id }));
                if (!response.Result.Succeeded)
                {
                    return new SendTwoFAQREmailResponse
                    {
                        Result = Result.Failure("The private code for the user couldn't be set.")
                    };
                }

                user.PrivateCode2FA = response?.Data?.PrivateCode2FA ?? string.Empty;
            }
            #endregion

            string title = _configuration.GetValue<string>("TwoFactorAuthentication:Enviroment") ?? throw new Exception("Enviroment not found.");

            SetupCode setupInfo = _tfa.GenerateSetupCode(user.UserName, title, user.PrivateCode2FA, false, 5);

            if (setupInfo is null)
            {
                return new SendTwoFAQREmailResponse
                {
                    Result = Result.Failure("The token could not be generated.")
                };
            }

            _barCodeImage = setupInfo.QrCodeSetupImageUrl;
            _manualCode = setupInfo.ManualEntryKey;
            _currentToken = _tfa.GetCurrentPIN(user.PrivateCode2FA);

            #region Set Attachments data
            string sContentId = "cidAuthenticationBarCodeImage";

            string AttExampleiPhonePath = _assetsService.GetAssetPath("TwoFactorAuthentication/Screenshot_iPhone.jpg");
            string sContentId_SH_iPhone = "cid_SH_iPhone";
            if (!System.IO.File.Exists(AttExampleiPhonePath))
            {
                throw new Exception($"File '{AttExampleiPhonePath}' not found.");
            }

            string AttExampleAndroidPath = _assetsService.GetAssetPath("TwoFactorAuthentication/Screenshot_Android.jpg");
            string sContentId_SH_Android = "cid_SH_Android";
            if (!System.IO.File.Exists(AttExampleAndroidPath))
            {
                throw new Exception($"File '{AttExampleAndroidPath}' not found.");
            }
            var base64Data = Regex.Match(_barCodeImage, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
            #endregion

            string body = _assetsService.GetAssetTextContent("TwoFactorAuthentication/TwoFAQR.html");
            body = body.Replace("{fullName}", $"{user.FirstName} {user.LastName}");
            body = body.Replace("{manualCode}", $"{_manualCode}");
            body = body.Replace("{iPhoneImg}", $"{sContentId_SH_iPhone}");
            body = body.Replace("{androidImg}", $"{sContentId_SH_Android}");
            body = body.Replace("{cidAuthenticationBarCodeImage}", $"{sContentId}");

            var attachments = new List<AttachmentDto>
            {
                AttachmentDto.CreateFromBase64(base64Data, "QR.jpg", sContentId),
                AttachmentDto.CreateFromPath(AttExampleiPhonePath, "Screenshot_iPhone.jpg", sContentId_SH_iPhone),
                AttachmentDto.CreateFromPath(AttExampleAndroidPath, "Screenshot_Android.jpg", sContentId_SH_Android)
            };

            var result = await _emailService.Send(user.Email!, "IFPI GMR Shop - User Code Verification", body, true, false, attachments);
            if (!result.Succeeded)
            {
                return new SendTwoFAQREmailResponse
                {
                    Result = Result.Failure("The email could not be sent.")
                };
            }

            return new SendTwoFAQREmailResponse
            {
                Result = Result.Success()
            };

        }
        catch (Exception Ex)
        {
            return new SendTwoFAQREmailResponse
            {
                Result = Result.Failure(Ex.Message)
            };
        }
    }
}