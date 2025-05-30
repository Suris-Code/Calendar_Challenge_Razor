using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Application.Common.Interfaces;
using Infrastructure.Identity.TwoFactorAuthentication.Contracts;
using Domain.Enums;
using Infrastructure.Identity.Welcome.Contracts;

public record VerifyEmailCommand(VerifyEmailRequest Request) : IRequest<VerifyEmailResponse>;

public class VerifyEmailCommandHandler(
    UserManager<ApplicationUser> userManager,
    IUserActivityLogService userActivityLogService,
    IMediator mediator) : IRequestHandler<VerifyEmailCommand, VerifyEmailResponse>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IUserActivityLogService _userActivityLogService = userActivityLogService;
    private readonly ISender _mediator = mediator;

    public async Task<VerifyEmailResponse> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(command.Request.Email);
            if (user == null)
            {
                return VerifyEmailResponse.Failure("User not found");
            }

            var storedCode = await _userManager.GetAuthenticationTokenAsync(
                user,
                "EmailConfirmation",
                "VerificationCode"
            );

            if (storedCode != command.Request.Code)
            {
                return VerifyEmailResponse.Failure("Invalid verification code");
            }

            user.EmailConfirmed = true;
            user.Enabled = YesNo.Yes;
            await _userManager.UpdateAsync(user);

            Console.WriteLine($"Stored Code: {storedCode}");

            await _userManager.RemoveAuthenticationTokenAsync(
                user,
                "EmailConfirmation",
                "VerificationCode"
            );

            await _userActivityLogService.LogUserActivityAsync(
                "Verify Email",
                $"Email verified for user {user.Email}",
                cancellationToken
            );

            if (user.LoginMethod2FA == LoginMethod2FA.App)
            {
                await _mediator.Send(new SendTwoFAQREmailCommand(new SendTwoFAQREmailRequest { UserId = user.Email! }));
            }

            return VerifyEmailResponse.Success();
        }
        catch (Exception e)
        {
            await _userActivityLogService.LogUserActivityAsync(
                "Verify Email",
                $"An error occurred while verifying the email: {e.Message}",
                cancellationToken
            );
            return VerifyEmailResponse.Failure("An error occurred while verifying the email");
        }
    }
}