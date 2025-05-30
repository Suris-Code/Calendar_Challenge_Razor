using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Services;

namespace Web.Controllers
{
    [ServiceFilter(typeof(LogActionAttribute))]
    public class BaseController : Controller
    {
        protected readonly ILogger<BaseController> _logger;
        protected readonly ISender _mediator;

        public BaseController(ILogger<BaseController> logger, ISender mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }
    }
} 