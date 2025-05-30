using Application.Common.Mappings;
using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Dtos.Users
{
    public class LoggedInUserDto : IMapFrom<ApplicationUser>
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string Email { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public List<string> Roles { get; set; }

        //public YesNo? TermsAndConditionsAccepted { get; set; }
        //public string? TermsAndConditionsAcceptedDescription => this.TermsAndConditionsAccepted.Description();
        public YesNo? ForcePasswordChange { get; set; }
        public string? ForcePasswordChangeDescription => ForcePasswordChange?.Description();
        public string? FullName => FirstName + " " + LastName;
        //public LoginMethod2FA LoginMethod2FA { get; set; }
        public YesNo EnableTwoFA { get; set; }
    }
}
