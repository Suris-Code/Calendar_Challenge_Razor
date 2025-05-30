using Application.Common.Interfaces;
using Domain.Entities;

public class ActivityLogService(IApplicationDbContext context, ICurrentUserService currentUserService, IDateTime dateTime) : IActivityLogService
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IDateTime _dateTime = dateTime;

    public async Task LogActivityAsync(
        string area,
        string controllerName,
        string actionName,
        string displayName,
        string ipAddress,
        string actionDescriptorId,
        string name,
        string parameters,
        string localIpAddress,
        string remotePort,
        string localPort,
        int? activityId,
        string activityReference,
        string userAgentFamily,
        string userAgentMajor,
        string userAgentMinor,
        string userAgentPatch,
        string osFamily,
        string osMajor,
        string osMinor,
        string osPatch,
        string deviceFamily,
        string deviceBrand,
        bool deviceIsSpider,
        string deviceModel
    )
    {
        try
        {
            var log = new ActivityLog
            {
                Date = _dateTime.UtcNow,
                UserId = string.IsNullOrEmpty(_currentUserService.UserId) ? null : _currentUserService.UserId,
                FullUserName = string.IsNullOrEmpty(_currentUserService.UserName) ? "anonymous" : _currentUserService.UserName,
                Area = area,
                ControllerName = controllerName,
                ActionName = actionName,
                DisplayName = displayName,
                ActionDescriptorId = actionDescriptorId,
                Name = name,
                Parameters = parameters,
                RemoteIPAddress = ipAddress,
                LocalIPAddress = localIpAddress,
                RemotePort = remotePort,
                LocalPort = localPort,
                ActivityId = activityId,
                ActivityReference = activityReference,
                UserAgentFamily = userAgentFamily,
                UserAgentMajor = userAgentMajor,
                UserAgentMinor = userAgentMinor,
                UserAgentPatch = userAgentPatch,
                OSFamily = osFamily,
                OSMajor = osMajor,
                OSMinor = osMinor,
                OSPatch = osPatch,
                DeviceFamily = deviceFamily,
                DeviceBrand = deviceBrand,
                DeviceIsSpider = deviceIsSpider,
                DeviceModel = deviceModel,
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {

            throw;
        }
    
    }
} 