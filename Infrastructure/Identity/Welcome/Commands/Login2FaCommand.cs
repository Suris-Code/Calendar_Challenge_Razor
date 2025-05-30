using Application.Common.Dtos.Users;
using Application.Common.Interfaces;
using Application.Logging.ErrorLogs.Commands;
using Application.Logging.ErrorLogs.Contracts;
using AutoMapper;
using Domain.Common.Models;
using Domain.Entities;
using Domain.Enums;
using Google.Authenticator;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Identity.Welcome.Commands
{
    public class Login2FaCommand : IRequest<(AuthResult response, LoggedInUserDto? user)>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string Code { get; set; }

        public Login2FaCommand(string email, string password, bool rememberMe, string code)
        {
            Email = email;
            Password = password;
            RememberMe = rememberMe;
            Code = code;
        }
    }

    public class Login2FaCommandHandler(UserManager<ApplicationUser> userManager, ISender mediator, IMapper mapper, IConfiguration configuration, TwoFactorAuthenticator tfa, IUserActivityLogService userActivityLogService) : IRequestHandler<Login2FaCommand, (AuthResult response, LoggedInUserDto? user)>
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly ISender _mediator = mediator;
        private readonly IMapper _mapper = mapper;
        private readonly IConfiguration _configuration = configuration;
        private readonly TwoFactorAuthenticator _tfa = tfa;
        private readonly IUserActivityLogService _userActivityLogService = userActivityLogService;

        public async Task<(AuthResult response, LoggedInUserDto? user)> Handle(Login2FaCommand request, CancellationToken cancellationToken)
        {
            var response = new AuthResult();

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
                    await _mediator.Send(new CreateErrorLogCommand(errorLog));

                    response.LoginOk = false;
                    response.Message = "Wrong username or password";

                    await _userActivityLogService.LogUserActivityAsync(
                        "Login",
                        $"User {request.Email} tried to login but account not found",
                        cancellationToken
                    );

                    return (response, null);
                }

                LoggedInUserDto userDto = _mapper.Map<LoggedInUserDto>(user);

                #region Check credentials
                if (!await _userManager.CheckPasswordAsync(user, request.Password))
                {
                    var newError = new CreateErrorLogRequest
                    {
                        Message = $"Incorrect credentials"
                    };
                    await _mediator.Send(new CreateErrorLogCommand(newError));
                    response.LoginOk = false;
                    response.Message = "Wrong username or password";

                    await _userActivityLogService.LogUserActivityAsync(
                        "Login",
                        $"User {request.Email} tried to login but password is invalid",
                        cancellationToken
                    );

                    return (response, null);
                }
                #endregion

                #region Check business rules

                if (user.EnableTwoFA == YesNo.No)
                {
                    response.LoginOk = false;
                    response.Message = "User's 2FA is disabled.";

                    await _userActivityLogService.LogUserActivityAsync(
                        "Login",
                        $"User {request.Email} tried to login but 2FA is disabled",
                        cancellationToken
                    );

                    return (response, null);
                }

                if (string.IsNullOrEmpty(request.Code))
                {
                    response.LoginOk = false;
                    response.Message = "Token is invalid.";

                    await _userActivityLogService.LogUserActivityAsync(
                        "Login",
                        $"User {request.Email} tried to login but token is invalid",
                        cancellationToken
                    );

                    return (response, null);
                }

                if (request.Code.Length != 6)
                {
                    response.LoginOk = false;
                    response.Message = "Token must be a six digit code.";

                    await _userActivityLogService.LogUserActivityAsync(
                        "Login",
                        $"User {request.Email} tried to login but token is invalid",
                        cancellationToken
                    );

                    return (response, null);
                }

                if (string.IsNullOrEmpty(user.PrivateCode2FA))
                {
                    response.LoginOk = false;
                    response.Message = "The private code for the user is not set.";

                    await _userActivityLogService.LogUserActivityAsync(
                        "Login",
                        $"User {request.Email} tried to login but private code is not set",
                        cancellationToken
                    );

                    return (response, null);
                }
                #endregion

                string sParametersMinutes = _configuration.GetValue<string>("TwoFactorAuthentication:ExpirePeriod") ?? "10";
                int parameterMinutes = int.Parse(sParametersMinutes);

                TimeSpan validTime = TimeSpan.FromMinutes(parameterMinutes);

                if (!_tfa.ValidateTwoFactorPIN(user.PrivateCode2FA, request.Code, validTime))
                {
                    response.LoginOk = false;
                    response.Message = "An error ocurred when validating the two factor authentication code.";

                    await _userActivityLogService.LogUserActivityAsync(
                        "Login",
                        $"User {request.Email} tried to login but two factor authentication code is invalid",
                        cancellationToken
                    );

                    return (response, null);
                }

                var loginResponse = await _mediator.Send(new LoginCommand(request.Email, request.Password, request.RememberMe, YesNo.No));

                if (!loginResponse.Result.Succeeded)
                {
                    response.LoginOk = false;
                    response.Message = "Login error ocurred when validating the two factor authentication code.";

                    await _userActivityLogService.LogUserActivityAsync(
                        "Login",
                        $"User {request.Email} tried to login but login error ocurred when validating the two factor authentication code",
                        cancellationToken
                    );

                    return (response, null);
                }

                user.LastLogin2FA = DateTime.Now;
                await _userManager.UpdateAsync(user);

                response.LoginOk = true;
                response.Message = "Login successful";

                await _userActivityLogService.LogUserActivityAsync(
                    "Login",
                    $"User {request.Email} logged in",
                    cancellationToken
                );

                return (response, loginResponse.Data);
            }
            catch (Exception e)
            {
                var errorLog = new CreateErrorLogRequest
                {
                   Type = ErrorLogType.Exception,
                   Message = e.Message,
                };
                await _mediator.Send(new CreateErrorLogCommand(errorLog));

                response.LoginOk = false;
                response.Message = "Login is not available at this moment";

                await _userActivityLogService.LogUserActivityAsync(
                    "Login",
                    $"User {request.Email} tried to login but login is not available at this moment",
                    cancellationToken
                );

                return (response, null);
            }
        }
    }
}
