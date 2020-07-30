using Microsoft.AspNetCore.Mvc;

namespace WebsocketGateway.Controllers
{
    public class AirplaneController : Controller
    {
        public AirplaneController()
        {
            
        }
        
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}