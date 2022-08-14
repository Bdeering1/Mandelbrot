using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Mandelbrot.Client.ApiClients
{
    public class UpdateClient
    {
        private HubConnection connection { get; }

        public event Action<string> OnImageReceived;

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

            connection.On<string>("ReceiveImage", ImageReceived);

            connection.StartAsync();
        }

        public bool IsConnected =>
            connection?.State == HubConnectionState.Connected;

        public async Task Disconnect()
        {
            await connection.StopAsync();
        }

        private void ImageReceived(string bitmap)
        {
            OnImageReceived?.Invoke(bitmap);
        }
    }
}

