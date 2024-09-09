using DeYasnoTelegramBot.Infrastructure.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeYasnoTelegramBot.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VersionController : ControllerBase
{
    private readonly DeYasnoConfig _deYasnoConfig;

    public VersionController(DeYasnoConfig deYasnoConfig)
    {
        _deYasnoConfig = deYasnoConfig;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Redirect("https://dn720407.ca.archive.org/0/items/rick-roll/Rick%20Roll.mp4");
    }

    //[HttpGet("appsettings")]
    //public async Task<IActionResult> GetAppsettings()
    //{
    //    return Ok(_deYasnoConfig);
    //}
}
