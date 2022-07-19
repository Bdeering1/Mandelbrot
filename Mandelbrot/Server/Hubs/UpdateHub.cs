using System.Diagnostics;
using System.Drawing;
using Mandelbrot.Core;
using Mandelbrot.Server.Core;
using Microsoft.AspNetCore.SignalR;
using SkiaSharp;

namespace Mandelbrot.Server.Hubs
{
    public class UpdateHub : Hub
    {
        public async Task SendRequest()
        {
            List<Color> colors = ColorGenerator.GetGradients(200);

            var s = new Stopwatch();
            s.Start();
            var set = Convert.ToBase64String(SetGenerator.GetBitmap(colors, 500, 500).Encode(SKEncodedImageFormat.Png, 100).ToArray());
            s.Stop();
            Console.WriteLine($"{s.ElapsedMilliseconds} ms elapsed");

            await Clients.All.SendAsync("ReceiveBitmap", set);
        }
    }
}

