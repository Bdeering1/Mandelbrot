using System.Diagnostics;
using Mandelbrot.Server.Core;
using Mandelbrot.Server.Hubs;
using Mandelbrot.Shared.Configuration;
using Mandelbrot.Shared.DTOs;
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

    [HttpGet]
    [Route("imageHttp")]
    public async Task<ImageDto> ImageRequestHttp()
    {
        SyncRunningParamters();
        generator.Reset();

        var s = new Stopwatch();
        s.Start();
        var image = Convert.ToBase64String((await generator.GetBitmap()).Encode(SKEncodedImageFormat.Png, 100).ToArray());
        s.Stop();
        Console.WriteLine($"{s.ElapsedMilliseconds / 1000.0}s elapsed");

        var dto = new ImageDto(image, (double)Config.Position.r, (double)Config.Position.i, Config.Zoom, Config.Precision, Config.MaxIterations);
        return dto;
    }

    [HttpPost]
    [Route("image")]
    public async Task ImageRequest()
    {
        SyncRunningParamters();
        generator.Reset();

        var s = new Stopwatch();
        s.Start();
        var image = Convert.ToBase64String((await generator.GetBitmap()).Encode(SKEncodedImageFormat.Png, 100).ToArray());
        s.Stop();
        Console.WriteLine($"{s.ElapsedMilliseconds / 1000.0}s elapsed");

        var dto = new ImageDto(image, (double)Config.Position.r, (double)Config.Position.i, Config.Zoom, Config.Precision, Config.MaxIterations);
        await hub.SendImage(dto);
    }

    private static void SyncRunningParamters()
    {
        BigDecimal.Precision = Config.Precision;
    }
}