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
    }
}

