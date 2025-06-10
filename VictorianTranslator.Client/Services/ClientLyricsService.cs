using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VictorianTranslator.Client.Services
{
    public class ClientLyricsService
    {
        private readonly HttpClient _httpClient;

        public ClientLyricsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<string>> GetAvailableSongsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<string>>("Lyrics/available") ?? new List<string>();
        }

        public async Task<string> GetLyricsAsync(string songFileName)
        {
            return await _httpClient.GetStringAsync($"Lyrics/lyrics/{songFileName}");
        }
    }
}
