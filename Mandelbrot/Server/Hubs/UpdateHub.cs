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
            var set = GenerateSet.GetBitmap(colors, 50, 50).Encode(SKEncodedImageFormat.Webp, 100).AsStream();

            await Clients.All.SendAsync("ReceiveBitmap", set);
            Console.WriteLine("Bitmap sent");
        }
    }
}

