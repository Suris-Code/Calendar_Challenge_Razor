using Domain.Enums;

namespace Infrastructure.Identity.Users.Contracts;

public class CreateUserRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
    public YesNo Enabled { get; init; }
    public YesNo Enable2FA { get; init; }
} 