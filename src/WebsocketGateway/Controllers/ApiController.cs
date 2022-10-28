using Microsoft.AspNetCore.Mvc;
using OgnGateway.Dtos;
using WebsocketGateway.Dtos;
using WebsocketGateway.Providers;

namespace WebsocketGateway.Controllers
{
    [Route("api")]
    public class ApiController : Controller
    {
        private readonly ILatestDataProvider _latestDataProvider;
        private readonly GatewayConfiguration _gatewayConfiguration;
        private readonly AprsConfig _aprsConfig;

        public ApiController(
            ILatestDataProvider latestDataProvider,
            GatewayConfiguration gatewayConfiguration,
            AprsConfig aprsConfig
        )
        {
            _latestDataProvider = latestDataProvider;
            _gatewayConfiguration = gatewayConfiguration;
            _aprsConfig = aprsConfig;
        }

        [HttpGet("current")]
        public IActionResult GetCurrentlyActiveAsync()
        {
            return Json(_latestDataProvider.GetLatestData());
        }

        [HttpGet("config")]
        public IActionResult GetConfiguration()
        {
            return Json(new
            {
                _gatewayConfiguration.MaxAgeSeconds,
                _gatewayConfiguration.EventsOnly,
                _gatewayConfiguration.IntervalSeconds,
                _aprsConfig.FilterPosition,
                _aprsConfig.FilterRadius
            });
        }
    }
}
