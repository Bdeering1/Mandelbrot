using System;
using System.Drawing;
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

        public async Task<List<Color>> GetColors()
        {
            var res = await http.GetFromJsonAsync<List<Color>>("colors");
            return res;
        }
    }
}

