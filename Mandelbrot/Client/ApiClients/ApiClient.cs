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
    }
}

