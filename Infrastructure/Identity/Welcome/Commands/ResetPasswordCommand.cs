using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Identity.Welcome.Contracts;

namespace Infrastructure.Identity.Welcome.Commands;

public record ResetPasswordCommand(ResetPasswordRequest Request) : IRequest<ResetPasswordResponse>;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserActivityLogService _userActivityLogService;

    public ResetPasswordCommandHandler(UserManager<ApplicationUser> userManager, IUserActivityLogService userActivityLogService)
    {
        _userManager = userManager;
        _userActivityLogService = userActivityLogService;
    }

    public async Task<ResetPasswordResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Request.Email);
        if (user == null)
        {
            return ResetPasswordResponse.Failure("Invalid request");
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Request.Token, request.Request.NewPassword);
        if (!result.Succeeded)
        {
            return ResetPasswordResponse.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        await _userActivityLogService.LogUserActivityAsync(
            "Reset Password",
            $"Password reset for user {user.Email}",
            cancellationToken
        );

        return ResetPasswordResponse.Success();
    }
}