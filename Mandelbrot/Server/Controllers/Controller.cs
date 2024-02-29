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

    private SetGenerator Generator { get; }
    private Camera Camera { get; }
    private UpdateHub Hub { get; }
    private ILogger<Controller> Logger { get; }

    public Controller(SetGenerator generator, Camera camera, UpdateHub hub, ILogger<Controller> logger)
    {
        Generator = generator;
        Camera = camera;
        Hub = hub;
        Logger = logger;
    }

    [HttpGet]
    [Route("imageHttp")]
    public async Task<ImageDto> ImageRequestHttp([FromQuery] double zoom, [FromQuery] double posX, [FromQuery] double posY)
    {
        Config.Zoom = zoom;
        Camera.Position = new BigComplex(posX, posY);

        SyncRunningParamters();
        Generator.Reset();

        var s = new Stopwatch();
        s.Start();
        var image = Convert.ToBase64String((await Generator.GetBitmap()).Encode(SKEncodedImageFormat.Png, 100).ToArray());
        s.Stop();
        Console.WriteLine($"{s.ElapsedMilliseconds / 1000.0}s elapsed");

        var dto = new ImageDto(image, (double)Camera.Position.R, (double)Camera.Position.I, Config.Zoom, Config.Precision, Config.MaxIterations);
        return dto;
    }

    [HttpPost]
    [Route("image")]
    public async Task ImageRequest()
    {
        SyncRunningParamters();
        Generator.Reset();

        var s = new Stopwatch();
        s.Start();
        var image = Convert.ToBase64String((await Generator.GetBitmap()).Encode(SKEncodedImageFormat.Png, 100).ToArray());
        s.Stop();
        Console.WriteLine($"{s.ElapsedMilliseconds / 1000.0}s elapsed");
        Console.WriteLine(image);

        var dto = new ImageDto(image, (double)Camera.Position.R, (double)Camera.Position.I, Config.Zoom, Config.Precision, Config.MaxIterations);
        await Hub.SendImage(dto);
    }

    private static void SyncRunningParamters()
    {
        BigDecimal.Precision = Config.Precision;
    }
}