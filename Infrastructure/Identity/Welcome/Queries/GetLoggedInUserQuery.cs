using Application.Common.Dtos.Users;
using Application.Common.Interfaces;
using Application.Common.Utils;
using AutoMapper;
using Domain.Common.Models;
using Domain.Entities;
using Infrastructure.Identity.Welcome.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Runtime.Intrinsics.X86;

namespace Infrastructure.Identity.Welcome.Queries;

public class GetLoggedInUserQuery() : IRequest<LoginResponse>
{
}

public class GetLoggedInUserQueryHandler(
    ICurrentUserService currentUserService,
    UserManager<ApplicationUser> userManager,
    IMapper mapper) : IRequestHandler<GetLoggedInUserQuery, LoginResponse>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IMapper _mapper = mapper;
    public async Task<LoginResponse> Handle(GetLoggedInUserQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUserService.Email))
            return LoginResponse.Failure("Not autenticated");

        var user = await _userManager.FindByEmailAsync(_currentUserService.Email);
        if (user == null)
            return LoginResponse.Failure("Not autenticated");

        LoggedInUserDto userDto = _mapper.Map<LoggedInUserDto>(user);
        var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
        userDto.Roles = roles?.Select(x=> x.ToLower()).ToList() ?? new List<string>();
        return LoginResponse.Success(userDto);
    }
}