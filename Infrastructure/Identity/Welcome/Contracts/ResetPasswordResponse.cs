using Application.Common.Models;

namespace Infrastructure.Identity.Welcome.Contracts;

public class ResetPasswordResponse : Response
{
    public static ResetPasswordResponse Success()
    {
        return new ResetPasswordResponse
        {
            Result = Result.Success()
        };
    }

    public static ResetPasswordResponse Failure(string error)
    {
        return new ResetPasswordResponse
        {
            Result = Result.Failure(error)
        };
    }
}