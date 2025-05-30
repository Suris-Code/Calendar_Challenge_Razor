using Application.Common.Dtos.Users;
using Application.Common.Interfaces;
using Application.Logging.ErrorLogs.Commands;
using Application.Logging.ErrorLogs.Contracts;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Identity.TwoFactorAuthentication.Contracts;
using Infrastructure.Identity.Welcome.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Infrastructure.Identity.Welcome.Commands
{
    public class LoginCommand(
        string email,
        string password,
        bool rememberMe,
        YesNo check2FA = YesNo.Yes) : IRequest<LoginResponse>
    {
        public string Email { get; set; } = email;
        public string Password { get; set; } = password;
        public bool RememberMe { get; set; } = rememberMe;
        public YesNo Check2FA { get; set; } = check2FA;
    }

    public class LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ISender mediator,
        SignInManager<ApplicationUser> signInManager,
        IMapper mapper,
        IUserActivityLogService userActivityLogService) : IRequestHandler<LoginCommand, LoginResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
        private readonly ISender _mediator = mediator;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly IMapper _mapper = mapper;
        private readonly IUserActivityLogService _userActivityLogService = userActivityLogService;

        public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(request.Email);

                if (user == null)
                {
                    var errorLog = new CreateErrorLogRequest
                    {
                        Type = ErrorLogType.NotFound,
                        Message = $"Account {request.Email} not found",
                    };
                    await _mediator.Send(new CreateErrorLogCommand(errorLog), cancellationToken);

                    return LoginResponse.Failure("Wrong username or password");
                }

                LoggedInUserDto userDto = _mapper.Map<LoggedInUserDto>(user);
                List<Claim> userClaims = [];

                var claimsByUser = await _userManager.GetClaimsAsync(user).ConfigureAwait(false);

                if (claimsByUser != null && claimsByUser.Count > 0)
                    userClaims.AddRange(claimsByUser.ToList());

                var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);

                if (roles != null && roles.Count > 0)
                {
                    foreach (var roleName in roles)
                    {
                        ApplicationRole? role = await _roleManager.FindByNameAsync(roleName);
                        if (role is null) continue;

                        var claimsByRole = await _roleManager.GetClaimsAsync(role);
                        if (claimsByRole != null && claimsByRole.Count > 0)
                            userClaims.AddRange(claimsByRole.ToList());
                    }
                }

                userDto.Roles = roles?.Select(x=> x.ToLower()).ToList() ?? new List<string>();

                if (userClaims == null || !userClaims.Any(c => c.Type == PolicyClaim.LoggedIn.ToString() && c.Value == "true"))
                {
                    await _userActivityLogService.LogUserActivityAsync(
                        "Login",
                        $"User {user.Email} not authorized",
                        cancellationToken
                    );

                    return LoginResponse.Failure("User not authorized");
                }

                SignInResult signInResult = new();
                //var userPasswordPolicy = await _mediator.Send(new GetPasswordPolicyByIdQuery(user.PasswordPolicyId));

                if (!await _userManager.CheckPasswordAsync(user, request.Password) || await _userManager.IsLockedOutAsync(user))
                {
                    //To take into consideration the failed login count
                    signInResult = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, true);
                }
                else
                {
                    #region Validate that the user has at least one role

                    var hasAnyRole = await _userManager.GetRolesAsync(user);

                    if (!hasAnyRole.Any())
                    {
                        await _userActivityLogService.LogUserActivityAsync(
                            "Login",
                            $"User {user.Email} not authorized",
                            cancellationToken
                        );

                    return LoginResponse.Failure("Your are unable to log in to GMR Shop. Please contact to administrator.");
                    }

                    #endregion

                    #region Disabled account

                    if (user.Enabled == YesNo.No)
                    {
                        await _userActivityLogService.LogUserActivityAsync(
                            "Login",
                            $"User {user.Email} is disabled",
                            cancellationToken
                        );

                        return LoginResponse.Failure("Your are unable to log in to GMR Shop. Please check your email for further information.");
                    }
                    #endregion

                    #region If user lockout end has passed, it returns to null

                    if (user.LockoutEnd < DateTime.Now)
                    {
                        user.LockoutEnd = null;
                    }
                    #endregion

                    #region Two factor validations
                    if (request.Check2FA == YesNo.Yes && user.EnableTwoFA == YesNo.Yes)
                    {
                        #region 2FA frequency
                        if (user.LoginMethod2FA == LoginMethod2FA.Email)
                        {
                            await _mediator.Send(new SendTwoFATokenEmailCommand(new SendTwoFATokenEmailRequest { UserId = user.Email! }));
                        }

                        await _userActivityLogService.LogUserActivityAsync(
                            "Login",
                            $"User {user.Email} is logged in with 2FA",
                            cancellationToken
                        );

                        return LoginResponse.Success(userDto);
                        #endregion
                    }

                    #endregion

                    signInResult = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, true);

                    #region Login OK and Record login in activity log

                    if (signInResult.Succeeded)
                    {
                        await _userActivityLogService.LogUserActivityAsync(
                            "Login",
                            $"User {user.Email} is logged in",
                            cancellationToken
                        );
                    }
                    #endregion

                    return LoginResponse.Success(userDto);
                }

                //throw new NotImplementedException("signInResult.RequiresTwoFactor");

                if (signInResult.IsNotAllowed)
                {
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        await _userActivityLogService.LogUserActivityAsync(
                            "Login",
                            $"User {user.Email} tried to login but email is not confirmed",
                            cancellationToken
                        );
                    }

                    return LoginResponse.Failure("Your are unable to log in to GMR Shop. Please contact the system administrator for further information.");
                }

                if (signInResult.IsLockedOut)
                {
                    await _userActivityLogService.LogUserActivityAsync(
                        "Login",
                        $"User {user.Email} tried to login but is locked out",
                        cancellationToken
                    );

                    return LoginResponse.Failure("Your are unable to log in to GMR Shop. Please check your email for further information.");
                }
                else
                {
                    await _userActivityLogService.LogUserActivityAsync(
                        "Login",
                        $"User {user.Email} tried to login but password is invalid",
                        cancellationToken
                    );

                    return LoginResponse.Failure("Wrong username or password");
                }

            }
            catch (Exception e)
            {
                var errorLog = new CreateErrorLogRequest
                {
                    Type = ErrorLogType.Exception,
                    Message = e.Message,
                };
                await _mediator.Send(new CreateErrorLogCommand(errorLog));

                return LoginResponse.Failure("Login is not available at this moment");
            }
        }
    }
}
