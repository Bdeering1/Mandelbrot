using Microsoft.AspNetCore.SignalR;

namespace Mandelbrot.Server.Hubs
{
    public class UpdateHub : Hub
    {
        public async Task SendImage(string set)
        {
            await Clients.All.SendAsync("ReceiveImage", set);
        }
    }
}

