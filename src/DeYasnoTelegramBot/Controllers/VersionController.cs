using DeYasnoTelegramBot.Infrastructure.Configurations;
using Microsoft.AspNetCore.Mvc;

namespace DeYasnoTelegramBot.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VersionController : ControllerBase
{
    private readonly DeYasnoConfig _deYasnoConfig;
    private readonly ILogger<VersionController> _logger;

    public VersionController(DeYasnoConfig deYasnoConfig,
        ILogger<VersionController> logger)
    {
        _deYasnoConfig = deYasnoConfig;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        _logger.LogInformation("RickRoll");
        return Redirect("https://dn720407.ca.archive.org/0/items/rick-roll/Rick%20Roll.mp4");
    }
}
