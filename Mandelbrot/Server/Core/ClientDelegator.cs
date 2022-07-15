using System;
using System.Drawing;
using Mandelbrot.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using SkiaSharp;

namespace Mandelbrot.Server.Core
{
    public class ClientDelegator
    {
        private HubConnection connection { get; }

        public ClientDelegator()
        {
            Console.WriteLine("Client delegator starting...");

            connection = new HubConnectionBuilder()
                .WithUrl("/hub")
                .WithAutomaticReconnect()
                .Build();

            connection.On<bool>("ReceiveRequest", RequestReceived);
        }

        public async Task Initialize()
        {
            Console.WriteLine("Initializing delegator");
            await connection.StartAsync();
            Console.WriteLine("Connection started in delegator");
        }

        private async Task RequestReceived(bool dummy)
        {
            Console.WriteLine("Request received");
            List<Color> colors = ColorGenerator.GetGradients(200);
            SKBitmap set = GenerateSet.GetBitmap(colors, 50, 50);
            await connection.SendAsync("SendBitmap", set);
            Console.WriteLine("Bitmap sent");
        }
    }
}

