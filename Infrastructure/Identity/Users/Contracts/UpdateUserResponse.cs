using Application.Common.Dtos.Users;
using Application.Common.Models;

namespace Infrastructure.Identity.Users.Contracts;

public class UpdateUserResponse : Response<UserDto>
{
    public static UpdateUserResponse Failure(IEnumerable<string> errors)
    {
        return new UpdateUserResponse
        {
            Result = Result.Failure(errors)
        };
    }

    public static UpdateUserResponse Success(UserDto user)
    {
        return new UpdateUserResponse
        {
            Result = Result.Success(),
            Data = user
        };
    }
} 