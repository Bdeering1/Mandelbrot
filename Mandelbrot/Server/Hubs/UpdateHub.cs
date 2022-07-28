using System.Diagnostics;
using Mandelbrot.Server.Core;
using Mandelbrot.Shared.Configuration;
using Mandelbrot.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using SkiaSharp;

namespace Mandelbrot.Server.Hubs
{
    public class UpdateHub : Hub
    {
        private SetGenerator generator { get; }

        public UpdateHub(SetGenerator generator)
        {
            this.generator = generator;
        }

        public async Task SendRequest()
        {
            SyncRunningParamters();

            var s = new Stopwatch();
            s.Start();
            var set = Convert.ToBase64String(generator.GetBitmap().Encode(SKEncodedImageFormat.Png, 100).ToArray());
            s.Stop();
            Console.WriteLine($"{s.ElapsedMilliseconds / 1000.0}s elapsed");

            await Clients.All.SendAsync("ReceiveBitmap", set);
        }

        private static void SyncRunningParamters()
        {
            BigDecimal.Precision = Config.Precision;
        }
    }
}

