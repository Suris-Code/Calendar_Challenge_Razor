using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services;
public class UserActivityLogService(IApplicationDbContext context, IDateTime dateTime, ICurrentUserService currentUserService, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor) : IUserActivityLogService
{
    private readonly IApplicationDbContext _context = context;
    private readonly IDateTime _dateTime = dateTime;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task LogUserActivityAsync(string action, string description, CancellationToken cancellationToken = default)
    {
        var username = "Anonymous";
        var currentUser = await _userManager.FindByIdAsync(_currentUserService.UserId);
        if (currentUser != null)
        {
            username = currentUser.UserName ?? username;
        }

        var log = new UserActivityLog
        {
            UserId = string.IsNullOrEmpty(_currentUserService.UserId) ? null : _currentUserService.UserId,
            UserName = username,
            Action = action,
            Description = description,
            IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            CreatedAt = _dateTime.UtcNow
        };

        _context.UserActivityLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
    }
}