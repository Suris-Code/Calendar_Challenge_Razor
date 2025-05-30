using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Application.Common.Interfaces;

namespace Web.Services;

public class LogActionAttribute : ActionFilterAttribute
{
    private IActivityLogService _logActivitySvc;

    public LogActionAttribute(IActivityLogService logActivitySvc)
    {
        _logActivitySvc = logActivitySvc;
    }

    public async override Task OnActionExecutionAsync(ActionExecutingContext filterContext, ActionExecutionDelegate next)
    {
        await next();

        var controller = filterContext.Controller as ControllerBase;

        if (controller == null)
        {
            return;
        }

        var controllerName = controller.ControllerContext.ActionDescriptor.ControllerName;
        var actionName = controller.ControllerContext.ActionDescriptor.ActionName;
        var displayName = controller.ControllerContext.ActionDescriptor.DisplayName;
        var id = controller.ControllerContext.ActionDescriptor.Id;
        var name = controller.ControllerContext.ActionDescriptor.MethodInfo.Name;
        var parameters = controller.ControllerContext.ActionDescriptor.Parameters;
        string stringParameters = "";
        foreach (var item in parameters)
        {
            stringParameters = stringParameters + item.Name + " ";
        }

        await _logActivitySvc.LogActivityAsync(
            string.Empty,
            controllerName,
            actionName,
            displayName ?? string.Empty,
            filterContext.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            id,
            name,
            stringParameters,
            filterContext.HttpContext.Connection.LocalIpAddress?.ToString() ?? "Unknown",
            filterContext.HttpContext.Connection.RemotePort.ToString() ?? "Unknown",
            filterContext.HttpContext.Connection.LocalPort.ToString() ?? "Unknown",
            0,
            string.Empty,
            filterContext.HttpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown",
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            false,
            string.Empty
        );

        base.OnActionExecuting(filterContext);
    }
} 