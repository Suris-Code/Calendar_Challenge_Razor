using System.ComponentModel.DataAnnotations;

namespace Domain.Enums
{
    public enum YesNo
    {
        [Display(Name = "No")]
        No = 0,
        [Display(Name = "Yes")]
        Yes = 1,
    }
}
