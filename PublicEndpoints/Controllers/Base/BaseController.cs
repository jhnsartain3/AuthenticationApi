using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Sartain_Studios_Common.Logging;

namespace PublicEndpoints.Controllers.Base
{
    public class BaseController : ControllerBase
    {
        private ILoggerWrapper _loggerWrapper;

        protected ILoggerWrapper LoggerWrapper =>
            _loggerWrapper ??= HttpContext?.RequestServices.GetService<ILoggerWrapper>();
    }
}