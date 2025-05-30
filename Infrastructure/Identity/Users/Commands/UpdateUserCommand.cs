using Application.Common.Dtos.Users;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Identity.Users.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Users.Commands;

public record UpdateUserCommand(UpdateUserRequest Request) : IRequest<UpdateUserResponse>;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UpdateUserResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public UpdateUserCommandHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<UpdateUserResponse> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var user = await _userManager.FindByIdAsync(request.Id.ToString());

        if (user == null)
        {
            return UpdateUserResponse.Failure(["User not found"]);
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.UserName = request.Email;
        user.Enabled = request.Enabled;
        user.TwoFactorEnabled = request.Enable2FA == YesNo.Yes;
        user.LoginMethod2FA = request.Enable2FA == YesNo.Yes ? LoginMethod2FA.Email : LoginMethod2FA.NotDefined;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return UpdateUserResponse.Failure(result.Errors.Select(e => e.Description));
        }

        if (!string.IsNullOrEmpty(request.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            result = await _userManager.ResetPasswordAsync(user, token, request.Password);

            if (!result.Succeeded)
            {
                return UpdateUserResponse.Failure(result.Errors.Select(e => e.Description));
            }
        }

        var userDto = _mapper.Map<UserDto>(user);
        return UpdateUserResponse.Success(userDto);
    }
} 