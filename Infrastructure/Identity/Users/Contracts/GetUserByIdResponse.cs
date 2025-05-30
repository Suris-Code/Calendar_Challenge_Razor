using Application.Common.Dtos.Users;
using Application.Common.Models;

namespace Infrastructure.Identity.Users.Contracts;

public class GetUserByIdResponse : Response<UserDto>
{
    public static GetUserByIdResponse Failure(IEnumerable<string> errors)
    {
        return new GetUserByIdResponse
        {
            Result = Result.Failure(errors)
        };
    }

    public static GetUserByIdResponse Success(UserDto user)
    {
        return new GetUserByIdResponse
        {
            Result = Result.Success(),
            Data = user
        };
    }
} 