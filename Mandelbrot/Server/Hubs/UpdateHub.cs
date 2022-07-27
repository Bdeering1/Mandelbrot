using System.Diagnostics;
using Mandelbrot.Core;
using Mandelbrot.Server.Core;
using Mandelbrot.Shared.Configuration;
using Microsoft.AspNetCore.SignalR;
using SkiaSharp;

namespace Mandelbrot.Server.Hubs
{
    public class UpdateHub : Hub
    {
        public async Task SendRequest()
        {
            var colors = ColorGenerator.GetBernsteinGradients(Config.MAX_ITERATIONS);

            var s = new Stopwatch();
            s.Start();
            var generator = new SetGenerator(1400, 800, colors);
            var set = Convert.ToBase64String(generator.GetBitmap().Encode(SKEncodedImageFormat.Png, 100).ToArray());
            s.Stop();
            Console.WriteLine($"{s.ElapsedMilliseconds / 1000.0}s elapsed");

            await Clients.All.SendAsync("ReceiveBitmap", set);
        }
    }
}

