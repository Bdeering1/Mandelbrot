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
    }
}

