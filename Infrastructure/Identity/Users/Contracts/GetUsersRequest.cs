using Application.Common.Models;
using Domain.Enums;

namespace Infrastructure.Identity.Users.Contracts;

public class GetUsersRequest : PaginatedWithFilter<GetUsersFilters>
{
}

public class GetUsersFilters
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public YesNo? Enabled { get; set; }
    public YesNo? Enable2FA { get; set; }
} 