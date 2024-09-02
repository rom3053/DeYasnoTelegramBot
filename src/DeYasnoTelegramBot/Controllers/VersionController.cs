using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeYasnoTelegramBot.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VersionController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok("Hello world");
    }
}
