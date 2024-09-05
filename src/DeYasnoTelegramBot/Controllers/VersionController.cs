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
        string htmlContent = @"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Test deploy</title>
        </head>
        <body>
            <h1>AutoPlay YouTube Video</h1>
            <iframe width='560' height='315' 
                src='https://www.youtube.com/embed/dQw4w9WgXcQ?&autoplay=1' 
                frameborder='0' 
                allow='autoplay; encrypted-media' 
                allowfullscreen>
            </iframe>
        </body>
        </html>";

        return Content(htmlContent, "text/html");
    }
}
