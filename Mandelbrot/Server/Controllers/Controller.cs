using System.Diagnostics;
using Mandelbrot.Server.Core;
using Mandelbrot.Server.Hubs;
using Mandelbrot.Shared.Configuration;
using Mandelbrot.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;

namespace Mandelbrot.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class Controller : ControllerBase
{

    private SetGenerator generator { get; }
    private UpdateHub hub { get; }
    private ILogger<Controller> logger { get; }

    public Controller(SetGenerator generator, UpdateHub hub, ILogger<Controller> logger)
    {
        this.logger = logger;
        this.generator = generator;
        this.hub = hub;
    }

    [HttpPost]
    [Route("image")]
    public async Task ImageRequest()
    {
        SyncRunningParamters();
        generator.Reset();

        var s = new Stopwatch();
        s.Start();
        var set = Convert.ToBase64String((await generator.GetBitmap()).Encode(SKEncodedImageFormat.Png, 100).ToArray());
        s.Stop();
        Console.WriteLine($"{s.ElapsedMilliseconds / 1000.0}s elapsed");

        await hub.SendImage(set);
    }

    private static void SyncRunningParamters()
    {
        BigDecimal.Precision = Config.Precision;
    }
}