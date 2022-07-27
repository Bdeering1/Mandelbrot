using System.Diagnostics;
using Mandelbrot.Server.Core;
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
            var s = new Stopwatch();
            s.Start();
            var set = Convert.ToBase64String(generator.GetBitmap().Encode(SKEncodedImageFormat.Png, 100).ToArray());
            s.Stop();
            Console.WriteLine($"{s.ElapsedMilliseconds / 1000.0}s elapsed");

            await Clients.All.SendAsync("ReceiveBitmap", set);
        }
    }
}

