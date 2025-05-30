namespace Infrastructure.Identity.Welcome.Contracts;

public class VerifyEmailRequest
{
    public string Email { get; set; }
    public string Code { get; set; }
}