using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;


        [Required]
        public YesNo Enabled { get; set; } = YesNo.No;

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        public DateTime LastModified { get; set; }

        public string LastModifiedBy { get; set; } = string.Empty;


        #region 2FA

        [Required]
        public LoginMethod2FA LoginMethod2FA { get; set; } = LoginMethod2FA.NotDefined;

        public string? PrivateCode2FA { get; set; }

        public DateTime? LastLogin2FA { get; set; }
        [Required]
        public YesNo EnableTwoFA { get; set; } = YesNo.No;

        #endregion
    }
}
