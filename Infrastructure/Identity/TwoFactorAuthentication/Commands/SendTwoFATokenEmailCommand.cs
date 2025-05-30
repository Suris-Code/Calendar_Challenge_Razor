using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using Google.Authenticator;
using Infrastructure.Identity.TwoFactorAuthentication.Commands;
using Infrastructure.Identity.TwoFactorAuthentication.Contracts;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;

public class SendTwoFATokenEmailCommand(SendTwoFATokenEmailRequest request) : IRequest<SendTwoFATokenEmailResponse>
{
    readonly SendTwoFATokenEmailRequest _request = request;

    public SendTwoFATokenEmailRequest Request => _request;
}

public class SendTwoFATokenEmailCommandHandler(UserManager<ApplicationUser> userManager, ISender mediator, IWebHostEnvironment webHostEnvironment, TwoFactorAuthenticator tfa, IEmailService emailService, IAssetsService assetsService) : IRequestHandler<SendTwoFATokenEmailCommand, SendTwoFATokenEmailResponse>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ISender _mediator = mediator;
    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
    private readonly TwoFactorAuthenticator _tfa = tfa;
    private readonly IEmailService _emailService = emailService;
    private readonly IAssetsService _assetsService = assetsService;

    public async Task<SendTwoFATokenEmailResponse> Handle(SendTwoFATokenEmailCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command.Request.UserId))
                return new SendTwoFATokenEmailResponse { Result = Result.Failure("User not found.") };

            var user = await _userManager.FindByEmailAsync(command.Request.UserId);
            if (user is null)
                return new SendTwoFATokenEmailResponse { Result = Result.Failure("Invalid user.") };

            #region If no privateCode2FA, generate it
            if (string.IsNullOrEmpty(user.PrivateCode2FA))
            {
                var response = await _mediator.Send(new ConfigureNewQRAppCommand(new ConfigureNewQRAppRequest { UserId = user.Id }));
                if (!response.Result.Succeeded || response.Data is null)
                    return new SendTwoFATokenEmailResponse { Result = Result.Failure("The private code for the user couldn't be set.") };

                user.PrivateCode2FA = response.Data.PrivateCode2FA;
            }
            #endregion

            var currentToken = _tfa.GetCurrentPIN(user.PrivateCode2FA);

            string body = _assetsService.GetAssetTextContent("TwoFactorAuthentication/TwoFAToken.html");
            body = body.Replace("{fullName}", $"{user.FirstName} {user.LastName}");
            body = body.Replace("{code}", $"{currentToken}");

            var result = await _emailService.Send(user.Email!, "IFPI GMR Shop - User Code Verification", body, true, false, null);

            if (!result.Succeeded)
                return new SendTwoFATokenEmailResponse { Result = Result.Failure("The email could not be sent.") };

            return new SendTwoFATokenEmailResponse { Result = Result.Success() };
        }
        catch (Exception Ex)
        {
            return new SendTwoFATokenEmailResponse { Result = Result.Failure(Ex.Message) };
        }
    }
}