using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Application.Common.Interfaces;

namespace Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        public string FirstName => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty;
        public string LastName => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Surname) ?? string.Empty;
        public string UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        public string UserName => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        public string IpAddress => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;
        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        public List<string> Claims => _httpContextAccessor.HttpContext?.User?.Claims.Select(x => x.Value).ToList() ?? new List<string>();
    }
} 