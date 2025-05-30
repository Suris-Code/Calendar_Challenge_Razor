using Application.Common.Models;

namespace Infrastructure.Identity.Welcome.Contracts
{
    public class RegisterResponse : Response
    {
        public static RegisterResponse Failure(string error)
        {
            return new RegisterResponse
            {
                Result = Result.Failure(error)
            };
        }

        public static RegisterResponse Success()
        {
            return new RegisterResponse
            {
                Result = Result.Success()
            };
        }
    }
}
