using System.ComponentModel.DataAnnotations;

namespace Domain.Enums
{
    public enum Policy
    {
        [Display(Name = "Logged-in")]
        LoggedIn = 0,
        [Display(Name = "Admin")]
        Admin = 1,
        [Display(Name = "Appointment Owner")]
        AppointmentOwner = 2
    }
}
