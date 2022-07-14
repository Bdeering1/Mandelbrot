using SkiaSharp;
using System.Net.Http.Json;

namespace Mandelbrot.Client
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
            SKBitmap bitmap;
            using (Stream stream = await http.GetStreamAsync("image"))
            using (MemoryStream memStream = new MemoryStream())
            {
                await stream.CopyToAsync(memStream);
                memStream.Seek(0, SeekOrigin.Begin);

                bitmap = SKBitmap.Decode(memStream);
            };
            return bitmap;
        }
    }
}

