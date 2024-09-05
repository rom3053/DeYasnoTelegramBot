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
        <style>
            .videoContainer {
                position: absolute;
                width: 100%;
                height: 100%;
                top: 0;
                left: 0;
                bottom: 0;
                right: 0;
                display: flex;
                flex-direction: column;
                justify-content: center;
                align-items: center;
            }

            iframe {
                width: 100%;
                height: 100%; 
            }
        </style>
    </head>
    <body>
        <div class='videoContainer'>
            <iframe width='560' height='315' 
                src='https://www.youtube.com/embed/XGxIE1hr0w4?autoplay=1&start=17' 
                frameborder='0' 
                allow='autoplay; encrypted-media' 
                allowfullscreen>
            </iframe>
        </div>
    </body>
    </html>";

        return Content(htmlContent, "text/html");
    }

}
