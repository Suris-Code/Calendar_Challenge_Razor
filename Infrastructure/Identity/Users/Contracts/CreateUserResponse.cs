using Application.Common.Dtos.Users;
using Application.Common.Models;

namespace Infrastructure.Identity.Users.Contracts;

public class CreateUserResponse : Response<UserDto>
{
    public static CreateUserResponse Failure(IEnumerable<string> errors)
    {
        return new CreateUserResponse
        {
            Result = Result.Failure(errors),
        };
    }

    public static CreateUserResponse Success(UserDto user)
    {
        return new CreateUserResponse
        {
            Result = Result.Success(),
            Data = user
        };
    }
} 