public interface IActivityLogService
{
    Task LogActivityAsync(
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
    );
} 