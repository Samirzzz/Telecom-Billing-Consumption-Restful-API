using Microsoft.AspNetCore.Mvc;

namespace TelecomBilling.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "Hello World 👋 from Telecom Billing API";
        }
    }
}
