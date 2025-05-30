using Application.Common.Dtos.Users;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Identity.Users.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Users.Queries;

public record GetUserByIdQuery(Guid Id) : IRequest<GetUserByIdResponse>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, GetUserByIdResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public GetUserByIdQueryHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<GetUserByIdResponse> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());

        if (user == null)
        {
            return GetUserByIdResponse.Failure(new[] { "Usuario no encontrado" });
        }

        var userDto = _mapper.Map<UserDto>(user);
        return GetUserByIdResponse.Success(userDto);
    }
} 