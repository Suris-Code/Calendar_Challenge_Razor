using Application.Common.Models;

namespace Infrastructure.Identity.Welcome.Contracts;

public class ForgotPasswordResponse : Response
{
    public static ForgotPasswordResponse Success()
    {
        return new ForgotPasswordResponse
        {
            Result = Result.Success()
        };
    }

    public static ForgotPasswordResponse Failure(string error)
    {
        return new ForgotPasswordResponse
        {
            Result = Result.Failure(error)
        };
    }
}