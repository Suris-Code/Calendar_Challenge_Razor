using Application.Common.Dtos.Users;
using Application.Common.Models;

namespace Infrastructure.Identity.TwoFactorAuthentication.Contracts;

public class SendTwoFATokenEmailResponse : Response<UserDto>
{
}