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
    public List<Color> GetColors()
    {
        var res = GenerateColors.GetGradients(20, 0.7);
        return res;
    }
}

