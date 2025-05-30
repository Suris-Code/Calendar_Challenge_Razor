using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Identity.Welcome.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Welcome.Commands;

public class LogoutCommand : IRequest<LogoutResponse>
{

    public LogoutCommand()
    {

    }
}

public class LogoutCommandHandler(SignInManager<ApplicationUser> signInManager, IUserActivityLogService userActivityLogService, ICurrentUserService currentUserService) : IRequestHandler<LogoutCommand, LogoutResponse>
{
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IUserActivityLogService _userActivityLogService = userActivityLogService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<LogoutResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {

        try
        {
            await _signInManager.SignOutAsync();

            await _userActivityLogService.LogUserActivityAsync(
                "Logout",
                $"User {_currentUserService.UserName} logged out",
                cancellationToken
            );

            return LogoutResponse.Success();
        }
        catch (Exception ex)
        {
            return LogoutResponse.Failure(ex.Message);
        }
    }
}