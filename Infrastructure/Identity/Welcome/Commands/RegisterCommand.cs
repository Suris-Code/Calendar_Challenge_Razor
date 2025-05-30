using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Identity.Welcome.Contracts;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Welcome.Commands;

public class RegisterCommand(RegisterRequest request) : IRequest<RegisterResponse>
{
    readonly RegisterRequest _request = request;
    public RegisterRequest Request => _request;
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserActivityLogService _userActivityLogService;
    private readonly IActivityLogService _activityLogService;
    private readonly IEmailService _emailService;
    private readonly IAssetsService _assetsService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        IUserActivityLogService userActivityLogService,
        IActivityLogService activityLogService,
        IEmailService emailService,
        IAssetsService assetsService,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _userActivityLogService = userActivityLogService;
        _activityLogService = activityLogService;
        _emailService = emailService;
        _assetsService = assetsService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = command.Request.Email,
            Email = command.Request.Email,
            FirstName = command.Request.Name,
            EnableTwoFA = YesNo.No,
        };

        var result = await _userManager.CreateAsync(user, command.Request.Password);
        if (!result.Succeeded)
        {
            return RegisterResponse.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        await _userManager.AddToRoleAsync(user, Role.User.ToString());

        var verificationCode = new Random().Next(100000, 999999).ToString();
        Console.WriteLine($"Generated Code: {verificationCode}");

        var setTokenResult = await _userManager.SetAuthenticationTokenAsync(
            user,
            "EmailConfirmation",
            "VerificationCode",
            verificationCode
        );

        Console.WriteLine($"Token Set Result: {setTokenResult.Succeeded}");

        var savedCode = await _userManager.GetAuthenticationTokenAsync(
            user,
            "EmailConfirmation",
            "VerificationCode"
        );

        Console.WriteLine($"Saved Code: {savedCode}");

        var requestUrl = _httpContextAccessor.HttpContext.Request;
        var baseUrl = $"{requestUrl.Scheme}://{requestUrl.Host}";
        var verifyUrl = $"{baseUrl}/verify-email/{Uri.EscapeDataString(user.Email)}/{Uri.EscapeDataString(verificationCode)}";

        await _emailService.Send(
            user.Email,
            "Verify your email",
            $@"
            <p>Hello {user.FirstName},</p>
            <p>You have successfully registered an account with us.</p>
            <p>To verify your email, please click the link below:</p>
            <p><a href='{verifyUrl}'>Verify Email</a></p>
            <p>If you didn't request this email verification, you can safely ignore this email. Your email will remain unchanged.</p>
            <p>This link will expire in 24 hours for security reasons.</p>
            ",
            true,
            null,
            null
        );

        await _userActivityLogService.LogUserActivityAsync(
            "Register",
            $"User registered with email {user.Email}",
            cancellationToken
        );

        return RegisterResponse.Success();
    }
}

