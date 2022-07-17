using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using SkiaSharp;

namespace Mandelbrot.Client.ApiClients
{
    public class UpdateClient
    {
        private HubConnection connection { get; }

        public event Action<string> OnBitmapReceived;

        public UpdateClient(NavigationManager navigationManager)
        {
            connection = new HubConnectionBuilder()
                .WithUrl(navigationManager.ToAbsoluteUri("/hub"))
                .WithAutomaticReconnect()
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
                    options.PayloadSerializerOptions.PropertyNamingPolicy = null;
                })
                .Build();

            connection.On<string>("ReceiveBitmap", BitmapReceived);

            connection.StartAsync();
        }

        public bool IsConnected =>
            connection?.State == HubConnectionState.Connected;

        public async Task Disconnect()
        {
            await connection.StopAsync();
        }

        public async Task SendRequest()
        {
            await connection.SendAsync("SendRequest"); // attach zoom/pos values heree
        }

        private void BitmapReceived(string bitmap)
        {
            Console.WriteLine("Received in client");
            OnBitmapReceived?.Invoke(bitmap);
        }
    }
}

