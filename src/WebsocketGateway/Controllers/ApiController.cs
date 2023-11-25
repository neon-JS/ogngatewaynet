namespace WebsocketGateway.Controllers;

[Route("api")]
public class ApiController(
    ILatestDataProvider latestDataProvider,
    GatewayConfiguration gatewayConfiguration,
    AprsConfig aprsConfig
) : Controller
{
    [HttpGet("current")]
    public IActionResult GetCurrentlyActiveAsync()
    {
        return Json(latestDataProvider.GetLatestData());
    }

    [HttpGet("config")]
    public IActionResult GetConfiguration()
    {
        return Json(new
        {
            gatewayConfiguration.MaxAgeSeconds,
            gatewayConfiguration.EventsOnly,
            gatewayConfiguration.IntervalSeconds,
            aprsConfig.FilterPosition,
            aprsConfig.FilterRadius
        });
    }
}