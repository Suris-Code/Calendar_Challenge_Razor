namespace Infrastructure.Identity.TwoFactorAuthentication.Contracts;

public class SendTwoFAQREmailRequest
{
    public string UserId { get; set; } = string.Empty;
} 