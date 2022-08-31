using System.Text.Json.Serialization;
using Mandelbrot.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Mandelbrot.Client.ApiClients
{
    public class UpdateClient
    {
        private HubConnection connection { get; }

        public event Action<ImageDto> OnImageReceived;

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

            connection.On<ImageDto>("ReceiveImage", ImageReceived);

            connection.StartAsync();
        }

        public bool IsConnected =>
            connection?.State == HubConnectionState.Connected;

        public async Task Disconnect()
        {
            await connection.StopAsync();
        }

        private void ImageReceived(ImageDto dto)
        {
            OnImageReceived?.Invoke(dto);
        }
    }
}

