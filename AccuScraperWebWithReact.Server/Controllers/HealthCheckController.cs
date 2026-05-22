using Microsoft.AspNetCore.Mvc;

namespace AccuScraperWebWithReact.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        [HttpGet]
        [HttpHead]
        public IActionResult Get() => Ok("Accu Web Scraper is healthy!");
    }
}
