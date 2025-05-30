using Application.Common.Mappings;
using Domain.Entities;
using Domain.Enums;
using System.Text.Json.Serialization;

namespace Application.Common.Dtos.Users;

public class UserDto : IMapFrom<ApplicationUser>
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    [JsonIgnore]
    public string PrivateCode2FA { get; set; } = string.Empty;
    public bool EnableTwoFA => EnableTwoFAEnum == YesNo.Yes;
    [JsonIgnore]
    public YesNo EnableTwoFAEnum { get; set; }
    public LoginMethod2FA LoginMethod2FA { get; set; }
    public YesNo Enabled { get; set; }
    
    [JsonIgnore]
    public bool SendTwoFAConfigureEmail { get; set; }
}
