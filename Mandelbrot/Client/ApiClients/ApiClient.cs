using Mandelbrot.Shared.DTOs;
using System.Net.Http.Json;

namespace Mandelbrot.Client.ApiClients
{
    public class ApiClient
    {
        private HttpClient http { get; }

        public ApiClient(HttpClient http)
        {
            this.http = http;
        }

        public async Task SendImageRequest()
        {
            await http.PostAsJsonAsync("image", "");
        }

        public async Task<ImageDto> SendImageRequestHttp()
        {
            var res = await http.GetFromJsonAsync<ImageDto>("imageHttp");
            return res;
        }
    }
}

