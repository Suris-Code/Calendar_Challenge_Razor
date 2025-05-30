using System.ComponentModel.DataAnnotations;

namespace Domain.Enums
{
    public enum Role
    {
        [Display(Name = "User")]
        User = 0,
        [Display(Name = "Administrator")]
        Admin = 1,
    }
}
