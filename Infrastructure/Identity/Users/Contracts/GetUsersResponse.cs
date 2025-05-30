using Application.Common.Dtos.Users;
using Application.Common.Models;

namespace Infrastructure.Identity.Users.Contracts;

public class GetUsersResponse : Response<UserDto[]>
{
    public int TotalCount { get; set; }

    public static GetUsersResponse Failure(IEnumerable<string> errors)
    {
        return new GetUsersResponse
        {
            Result = Result.Failure(errors)
        };
    }

    public static GetUsersResponse Success(UserDto[] users, int totalCount)
    {
        return new GetUsersResponse
        {
            Result = Result.Success(),
            Data = users,
            TotalCount = totalCount
        };
    }
} 