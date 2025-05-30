namespace Infrastructure.Identity.Welcome.Contracts;

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}