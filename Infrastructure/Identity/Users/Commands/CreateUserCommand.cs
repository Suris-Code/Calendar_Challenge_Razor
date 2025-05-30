using Application.Common.Dtos.Users;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Identity.Users.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Users.Commands;

public record CreateUserCommand(CreateUserRequest Request) : IRequest<CreateUserResponse>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public CreateUserCommandHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<CreateUserResponse> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Enabled = request.Enabled,
            TwoFactorEnabled = request.Enable2FA == YesNo.Yes,
            EmailConfirmed = true,
            LoginMethod2FA = request.Enable2FA == YesNo.Yes ? LoginMethod2FA.Email : LoginMethod2FA.NotDefined
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return CreateUserResponse.Failure(result.Errors.Select(e => e.Description));
        }

        var userDto = _mapper.Map<UserDto>(user);
        return CreateUserResponse.Success(userDto);
    }
} 