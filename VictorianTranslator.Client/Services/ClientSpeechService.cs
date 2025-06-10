using System.Net.Http;
using System.Net.Http.Json; // Added for PostAsJsonAsync
using System.Threading.Tasks;

namespace VictorianTranslator.Client.Services
{
    public class ClientSpeechService
    {
        private readonly HttpClient _httpClient;

        public ClientSpeechService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<byte[]> SynthesizeSpeechAsync(string text)
        {
            var response = await _httpClient.PostAsJsonAsync("Speech/synthesize", text);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
