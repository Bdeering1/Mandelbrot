using System.Drawing;
using Mandelbrot.Core;
using Mandelbrot.Server.Core;
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
        List<Color> res = ColorGenerator.GetGradients(200);
        var set = GenerateSet.MandelBrot(res, 500, 500).Select(x => ColorGenerator.GetHex(x)).ToList();
        return set;
    }
}

