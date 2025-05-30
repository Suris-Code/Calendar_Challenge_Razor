using Domain.Enums;

namespace Infrastructure.Identity.Users.Contracts;

public class UpdateUserRequest
{
    public required string Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public string? Password { get; init; }
    public YesNo Enabled { get; init; }
    public YesNo Enable2FA { get; init; }
} 