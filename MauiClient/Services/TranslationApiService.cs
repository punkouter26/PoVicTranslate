using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MauiClient.Services;

/// <summary>
/// Service for communicating with the backend translation API.
/// Implements the Repository Pattern for translation operations.
/// </summary>
public class TranslationApiService
{
    private readonly HttpClient _httpClient;
    private const string ApiBaseUrl = "https://localhost:7241"; // TODO: Make configurable

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslationApiService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used for API requests.</param>
    public TranslationApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(ApiBaseUrl);
    }

    /// <summary>
    /// Translates modern English text to Victorian English by calling the backend API.
    /// </summary>
    /// <param name="modernText">The modern English text to translate.</param>
    /// <returns>The translated Victorian English text, or an error message if translation fails.</returns>
    /// <exception cref="HttpRequestException">Thrown if the API call fails.</exception>
    public async Task<string> TranslateToVictorianEnglishAsync(string modernText)
    {
        Debug.WriteLine($"[TranslationApiService] Attempting translation for: {modernText}");
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/translate", new { text = modernText });
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<TranslationResult>();
            Debug.WriteLine($"[TranslationApiService] Translation result: {result?.VictorianText}");
            return result?.VictorianText ?? "Translation failed.";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TranslationApiService] Error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Represents the result returned by the translation API.
    /// </summary>
    private class TranslationResult
    {
        /// <summary>
        /// Gets or sets the translated Victorian English text.
        /// </summary>
        public string VictorianText { get; set; } = string.Empty;
    }
} 