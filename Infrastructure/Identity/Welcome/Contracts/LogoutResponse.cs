using Application.Common.Models;

namespace Infrastructure.Identity.Welcome.Contracts;

public class LogoutResponse : Response
{
    public static LogoutResponse Failure(string error)
    {
        return new LogoutResponse
        {
            Result = Result.Failure(error)
        };
    }

    public static LogoutResponse Success()
    {
        return new LogoutResponse
        {
            Result = Result.Success()
        };
    }
}
