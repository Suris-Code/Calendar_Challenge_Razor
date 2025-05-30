using System.ComponentModel.DataAnnotations;

namespace Domain.Enums
{
    public enum ErrorLogType
    {
        [Display(Name = "Not allowed")]
        NotAllowed = 0,
        [Display(Name = "Exception")]
        Exception = 1,
        [Display(Name = "HTTP Code")]
        HttpCode = 2,
        [Display(Name = "Not found")]
        NotFound = 3,
        [Display(Name = "Invalid enum")]
        InvalidEnum = 4,
        [Display(Name = "Other")]
        Other = 5,
    }
}
