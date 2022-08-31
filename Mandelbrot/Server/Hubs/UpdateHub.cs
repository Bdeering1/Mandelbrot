using Mandelbrot.Shared.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace Mandelbrot.Server.Hubs
{
    public class UpdateHub : Hub
    {
        public async Task SendImage(ImageDto dto)
        {
            await Clients.All.SendAsync("ReceiveImage", dto);
        }
    }
}

