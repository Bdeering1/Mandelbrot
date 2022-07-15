using System.Drawing;
using Mandelbrot.Core;
using Mandelbrot.Server.Core;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;

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

    //[HttpGet]
    //[Route("colors")]
    //public List<string> GetColors()
    //{
    //    List<Color> res = ColorGenerator.GetGradients(200);
    //    var set = GenerateSet.GetColorList(res, 500, 500).Select(x => ColorGenerator.GetHex(x)).ToList();
    //    return set;
    //}

    [HttpGet]
    [Route("image")]
    public SKBitmap GetImage()
    {
        List<Color> colors = ColorGenerator.GetGradients(200);
        SKBitmap set = GenerateSet.GetBitmap(colors, 50, 50);
        return set;
    }
}

