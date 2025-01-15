using System.Text;
using System.Text.Json;
using VictorianTranslator.Configuration;
using Microsoft.Extensions.Options;

namespace VictorianTranslator.Services;

public class TranslationService : ITranslationService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string GEMINI_API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";

    public TranslationService(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
    {
        _httpClient = httpClient;
        _apiKey = apiSettings.Value.GeminiApiKey;
    }

    public async Task<string> TranslateToVictorianEnglishAsync(string modernText)
    {
        var prompt = @$"
            Please translate the following modern English text into Victorian-era English, 
            maintaining proper Victorian mannerisms, formality, and eloquence. 
            
            Rules for translation:
            1. Use formal and elaborate language
            2. Include Victorian-era expressions and phrases
            3. Maintain proper etiquette in speech
            4. Use more sophisticated vocabulary
            5. Add appropriate honorifics where applicable

            Text to translate: '{modernText}'

            Provide only the translated text without any explanations or additional context.";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var requestJson = JsonSerializer.Serialize(requestBody);
        var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(
            $"{GEMINI_API_URL}?key={_apiKey}",
            requestContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Translation API error: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseContent);

        // Navigate through the response JSON to get the generated text
        var text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return text ?? "Translation failed to generate text.";
    }
}
