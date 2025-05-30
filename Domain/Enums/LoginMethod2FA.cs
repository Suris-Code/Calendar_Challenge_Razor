using System.ComponentModel.DataAnnotations;

namespace Domain.Enums;

public enum LoginMethod2FA
{
    [Display(Name = "Not defined")]
    NotDefined = 0,
    [Display(Name = "Email")]
    Email = 1,
    [Display(Name = "App")]
    App = 2,
}