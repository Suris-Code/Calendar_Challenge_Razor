using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Identity.Welcome.Contracts;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Identity.Welcome.Commands;

public class ForgotPasswordCommand(ForgotPasswordRequest request) : IRequest<ForgotPasswordResponse>
{
    private readonly ForgotPasswordRequest _request = request;
    public ForgotPasswordRequest Request => _request;
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserActivityLogService _userActivityLogService;

    public ForgotPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        IUserActivityLogService userActivityLogService)
    {
        _userManager = userManager;
        _emailService = emailService;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _userActivityLogService = userActivityLogService;
    }

    public async Task<ForgotPasswordResponse> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(command.Request.Email);
        if (user == null)
        {
            return ForgotPasswordResponse.Success();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var requestUrl = _httpContextAccessor.HttpContext.Request;
        var baseUrl = $"{requestUrl.Scheme}://{requestUrl.Host}";
        var resetUrl = $"{baseUrl}/reset-password/{Uri.EscapeDataString(user.Email)}/{Uri.EscapeDataString(token)}";

        await _emailService.Send(
            user.Email,
            "Reset Password",
            $@"
            <p>Hello,</p>
            <p>We received a request to reset your password for your account.</p>
            <p>To reset your password, please click the link below:</p>
            <p><a href='{resetUrl}'>Reset Password</a></p>
            <p>If you didn't request this password reset, you can safely ignore this email. Your password will remain unchanged.</p>
            <p>This link will expire in 24 hours for security reasons.</p>
            ",
            true,
            false,
            null
        );

        await _userActivityLogService.LogUserActivityAsync(
            "Forgot Password",
            $"User {user.Email} requested a password reset",
            cancellationToken
        );

        return ForgotPasswordResponse.Success();
    }
}