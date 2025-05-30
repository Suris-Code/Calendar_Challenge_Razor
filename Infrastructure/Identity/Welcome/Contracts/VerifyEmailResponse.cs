using Application.Common.Models;

namespace Infrastructure.Identity.Welcome.Contracts;

public class VerifyEmailResponse : Response
{
    public static VerifyEmailResponse Failure(string error)
    {
        return new VerifyEmailResponse
        {
            Result = Result.Failure(error)
        };
    }

    public static VerifyEmailResponse Success()
    {
        return new VerifyEmailResponse
        {
            Result = Result.Success()
        };
    }
}