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
}

