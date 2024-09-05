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
        return Redirect("https://dn720407.ca.archive.org/0/items/rick-roll/Rick%20Roll.mp4");
    }
}
