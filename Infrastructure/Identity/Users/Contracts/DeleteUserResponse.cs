using Application.Common.Models;

namespace Infrastructure.Identity.Users.Contracts;

public class DeleteUserResponse : Response<bool>
{
    public static DeleteUserResponse Failure(IEnumerable<string> errors)
    {
        return new DeleteUserResponse
        {
            Result = Result.Failure(errors),
            Data = false
        };
    }

    public static DeleteUserResponse Success()
    {
        return new DeleteUserResponse
        {
            Result = Result.Success(),
            Data = true
        };
    }
} 