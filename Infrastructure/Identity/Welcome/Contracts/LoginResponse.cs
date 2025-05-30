using Application.Common.Dtos.Users;
using Application.Common.Models;
using Domain.Enums;

namespace Infrastructure.Identity.Welcome.Contracts;

public class LoginResponse : Response<LoggedInUserDto>
{
    public static LoginResponse Failure(string error)
    {
        return new LoginResponse
        {
            Result = Result.Failure(error)
        };
    }

    public static LoginResponse Success(LoggedInUserDto user)
    {
        return new LoginResponse
        {
            Data = user,
            Result = Result.Success()
        };
    }
}
