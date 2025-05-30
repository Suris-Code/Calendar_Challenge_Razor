using System.ComponentModel.DataAnnotations;

namespace Domain.Common.Models;

public class UserLogin2FaRequest
{
    [Required]
    public required string Email { get; set; }
    [Required]
    public required string Password { get; set; }
    [Required]
    public bool RememberMe { get; set; }
    [Required]
    public required string Code { get; set; }
}
