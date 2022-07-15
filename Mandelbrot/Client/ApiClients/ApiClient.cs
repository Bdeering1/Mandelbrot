using SkiaSharp;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mandelbrot.Client.ApiClients
{
    public class ApiClient
    {
        private HttpClient http { get; }

        public ApiClient(HttpClient http)
        {
            this.http = http;
        }

        public async Task<List<string>> GetColors()
        {
            var res = await http.GetFromJsonAsync<List<string>>("colors");
            return res;
        }

        public async Task<SKBitmap> GetImage()
        {
            var res = await http.GetFromJsonAsync<SKBitmap>("image", new JsonSerializerOptions()
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                PropertyNamingPolicy = null
            });
            return res;
        }
    }
}

