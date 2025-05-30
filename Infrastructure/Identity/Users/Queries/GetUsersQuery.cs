using Application.Common.Dtos.Users;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Identity.Users.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Identity.Users.Queries;

public record GetUsersQuery(GetUsersRequest Request) : IRequest<GetUsersResponse>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, GetUsersResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public GetUsersQueryHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<GetUsersResponse> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _userManager.Users.AsQueryable();

        if (request.Request.Filters != null)
        {
            query = ApplyFilters(query, request.Request.Filters);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderBy(x => x.Email)
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(cancellationToken);

        var userDtos = _mapper.Map<UserDto[]>(users);

        return GetUsersResponse.Success(userDtos, totalCount);
    }

    private IQueryable<ApplicationUser> ApplyFilters(IQueryable<ApplicationUser> query, GetUsersFilters filters)
    {
        if (!string.IsNullOrEmpty(filters.Email))
            query = query.Where(x => x.Email!.Contains(filters.Email));

        if (!string.IsNullOrEmpty(filters.FirstName))
            query = query.Where(x => x.FirstName.Contains(filters.FirstName));

        if (!string.IsNullOrEmpty(filters.LastName))
            query = query.Where(x => x.LastName.Contains(filters.LastName));

        if (filters.Enabled.HasValue)
            query = query.Where(x => x.Enabled == filters.Enabled);

        if (filters.Enable2FA.HasValue)
            query = query.Where(x => x.TwoFactorEnabled == (filters.Enable2FA == YesNo.Yes));

        return query;
    }
} 