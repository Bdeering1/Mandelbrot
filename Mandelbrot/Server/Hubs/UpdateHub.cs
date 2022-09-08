using Mandelbrot.Shared.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace Mandelbrot.Server.Hubs
{
    public class UpdateHub : Hub
    {
        protected IHubContext<UpdateHub> _context;

        public UpdateHub(IHubContext<UpdateHub> context)
        {
            _context = context;
        }
        public async Task SendImage(ImageDto dto)
        {
            await _context.Clients.All.SendAsync("ReceiveImage", dto);
            Console.WriteLine("image sent");
        }
    }
}

