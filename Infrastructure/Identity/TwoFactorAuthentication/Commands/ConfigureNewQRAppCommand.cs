using Application.Common.Dtos.Users;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Logging.ErrorLogs.Commands;
using Application.Logging.ErrorLogs.Contracts;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Identity.TwoFactorAuthentication.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.TwoFactorAuthentication.Commands;

public class ConfigureNewQRAppCommand(ConfigureNewQRAppRequest request) : IRequest<ConfigureNewQRAppResponse>
{
    readonly ConfigureNewQRAppRequest _request = request;

    public ConfigureNewQRAppRequest Request => _request;
}

public class ConfigureNewQRAppCommandHandler(IApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager, ISender mediator, ICrypto crypto, IMapper mapper) : IRequestHandler<ConfigureNewQRAppCommand, ConfigureNewQRAppResponse>
{
    private readonly IApplicationDbContext _applicationDbContext = applicationDbContext;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ISender _mediator = mediator;
    private readonly ICrypto _crypto = crypto;
    private readonly IMapper _mapper = mapper;

    public async Task<ConfigureNewQRAppResponse> Handle(ConfigureNewQRAppCommand command, CancellationToken cancellationToken)
    {
        try
        {
            #region Find the user
            var user = await _userManager.FindByIdAsync(command.Request.UserId);
            if (user == null)
            {
                var newError = new CreateErrorLogRequest
                {
                    Message = $"User '{command.Request.UserId}' was not found for 2FA enabling"
                };

                await _mediator.Send(new CreateErrorLogCommand(newError));
                return new ConfigureNewQRAppResponse
                {
                    Result = Result.Failure("User's 2FA could not be enabled")
                };
            }
            #endregion

            #region Configure privatecode

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random oRandom = new Random();

            string privateCode = new string(Enumerable.Repeat(chars, 10)
              .Select(s => s[oRandom.Next(s.Length)]).ToArray());

            user.PrivateCode2FA = _crypto.EncryptString(privateCode);

            var identityResult = await _userManager.UpdateAsync(user);
            if (!identityResult.Succeeded)
            {
                foreach (var error in identityResult.Errors)
                {
                    var newError = new CreateErrorLogRequest
                    {
                        Message = $"Identity error {error.Code}: {error.Description}",
                        Type = ErrorLogType.Other
                    };
                    await _mediator.Send(new CreateErrorLogCommand(newError));
                }
                return new ConfigureNewQRAppResponse
                {
                    Result = Result.Failure("User's 2FA login method could not be changed")
                };
            }

            /*             #region Record activity log

                        await _mediator.Send(new CreateActivityLogCommand(Activity.TwoFAEnabled));

                        #endregion */

            #endregion
            return new ConfigureNewQRAppResponse
            {
                Result = Result.Success("User's 2FA successfully enabled"),
                Data = _mapper.Map<UserDto>(user)
            };
        }
        catch (Exception ex)
        {
            return new ConfigureNewQRAppResponse
            {
                Result = Result.Failure(ex.Message)
            };
        }
    }
}