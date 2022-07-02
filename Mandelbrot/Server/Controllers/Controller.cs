using System.Drawing;
using Mandelbrot.Core;
using Microsoft.AspNetCore.Mvc;

namespace Mandelbrot.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class Controller : ControllerBase
{
    private readonly ILogger<Controller> logger;

    public Controller(ILogger<Controller> logger)
    {
        this.logger = logger;
    }

    [HttpGet]
    [Route("colors")]
    public List<string> GetColors()
    {
        var res = ColorGenerator.GetGradients(200, 0.7).Select(x => ColorGenerator.GetHexString(x)).ToList();
        return res;
    }
}

