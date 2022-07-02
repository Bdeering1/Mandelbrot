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
        var res = GenerateColors.GetGradients(20, 0.7).Select(x => GetHexString(x)).ToList();
        return res;
    }

    private string GetHexString(Color c) =>
            $"#{c.R:X2}{c.G:X2}{c.B:X2}";
}

