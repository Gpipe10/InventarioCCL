using Microsoft.AspNetCore.Mvc;

namespace Inventario.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "OK",
                message = "API de inventario funcionando correctamente"
            });
        }
    }
}