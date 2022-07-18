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
            Console.WriteLine("Request received");
            List<Color> colors = ColorGenerator.GetGradients(200);

            Stopwatch s = new Stopwatch();
            s.Start();
            var set = Convert.ToBase64String(GenerateSet.GetBitmap(colors, 500, 500).Encode(SKEncodedImageFormat.Png, 100).ToArray());
            s.Stop();
            Console.WriteLine(s.ElapsedMilliseconds);

            await Clients.All.SendAsync("ReceiveBitmap", set);
            Console.WriteLine("Bitmap sent");
        }
    }
}

