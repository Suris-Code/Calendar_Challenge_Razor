namespace Infrastructure.Identity.TwoFactorAuthentication.Contracts;

public class SendTwoFATokenEmailRequest
{
    public string UserId { get; set; } = string.Empty;
} 