using Microsoft.AspNetCore.SignalR;
using SkiaSharp;

namespace Mandelbrot.Server.Hubs
{
    public class UpdateHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendBitmap(SKBitmap bitmap)
        {
            await Clients.All.SendAsync("ReceiveBitmap", bitmap);
        }

        public async Task SendRequest(bool dummy)
        {
            Console.WriteLine("Request received at hub");
            await Clients.All.SendAsync("ReceiveRequest", dummy);
        }
    }
}

