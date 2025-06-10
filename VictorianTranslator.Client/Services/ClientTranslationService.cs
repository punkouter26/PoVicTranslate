using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using VictorianTranslator.Client.Models;

namespace VictorianTranslator.Client.Services
{
    public class ClientTranslationService
    {
        private readonly HttpClient _httpClient;

        public ClientTranslationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> TranslateText(string text)
        {
            var request = new TranslationRequest { Text = text };
            var response = await _httpClient.PostAsJsonAsync("Translation/translate", request);
            response.EnsureSuccessStatusCode();
            var translationResponse = await response.Content.ReadFromJsonAsync<TranslationResponse>();
            return translationResponse?.TranslatedText;
        }
    }
}
