using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using SkiaSharp;

namespace Mandelbrot.Client.ApiClients
{
    public class UpdateClient
    {
        private HubConnection connection { get; }

        public event Action<SKBitmap> OnBitmapReceived;

        public UpdateClient(NavigationManager navigationManager)
        {
            connection = new HubConnectionBuilder()
                .WithUrl(navigationManager.ToAbsoluteUri("/hub"))
                .WithAutomaticReconnect()
                .Build();

            connection.On<SKBitmap>("ReceiveBitmap", BitmapReceived);

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
            await connection.SendAsync("SendRequest", true); // attach zoom/pos values heree
        }

        private void BitmapReceived(SKBitmap bitmap)
        {
            OnBitmapReceived?.Invoke(bitmap);
        }
    }
}

