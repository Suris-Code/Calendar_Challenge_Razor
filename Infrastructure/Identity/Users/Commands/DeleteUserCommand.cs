using Domain.Entities;
using Infrastructure.Identity.Users.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Users.Commands;

public record DeleteUserCommand(Guid Id) : IRequest<DeleteUserResponse>;

public class DeleteUserCommandHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<DeleteUserCommand, DeleteUserResponse>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<DeleteUserResponse> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());

        if (user == null)
        {
            return DeleteUserResponse.Failure(new[] { "Usuario no encontrado" });
        }

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            return DeleteUserResponse.Failure(result.Errors.Select(e => e.Description));
        }

        return DeleteUserResponse.Success();
    }
} 