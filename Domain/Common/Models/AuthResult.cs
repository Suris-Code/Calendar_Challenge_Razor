namespace Domain.Common.Models;
public class AuthResult
{
    public string Message { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string FullName => $"{FirstName} {LastName}";
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Initials { get; set; } = "";
    public string Profile { get; set; } = "";
    //public List<AuthAbility> Ability { get; set; } = new List<AuthAbility>();
    public bool LoginOk { get; set; } = false;  // false = Locked or disabled or wrong password or not allowed
    public bool AccountRenewalRequired { get; set; } = false;
    public bool FirstTimeLogin { get; set; } = false;
    public bool TermsAndConditionsNotAccepted { get; set; } = false;
    public bool TwoFactorNotRenewed { get; set; } = false;
    public bool TwoFactorFirstTime { get; set; } = false;
    public bool HasTwoFactorMethod { get; set; } = false;
    public bool ChangePasswordRequired { get; set; } = false;
    public string Cookie { get; set; }
    public bool EnableTwoFA { get; set; } = false;
}

