using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using PoVicTranslate.Web.Configuration;
using PoVicTranslate.Web.Services.Validation;

namespace PoVicTranslate.Web.Services;

/// <summary>
/// Azure Speech Services implementation for audio synthesis using REST API.
/// </summary>
public sealed class AudioSynthesisService : IAudioSynthesisService
{
    private readonly ApiSettings _settings;
    private readonly ISpeechConfigValidator _configValidator;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AudioSynthesisService> _logger;
    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    private const string VoiceName = "en-GB-RyanNeural";

    public AudioSynthesisService(
        IOptions<ApiSettings> apiSettings,
        ISpeechConfigValidator configValidator,
        IHttpClientFactory httpClientFactory,
        ILogger<AudioSynthesisService> logger)
    {
        ArgumentNullException.ThrowIfNull(apiSettings);
        _settings = apiSettings.Value;
        _configValidator = configValidator;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private async Task<string> GetAccessTokenAsync()
    {
        if (_accessToken is not null && DateTime.UtcNow < _tokenExpiry.AddMinutes(-1))
        {
            return _accessToken;
        }

        var tokenEndpoint = $"https://{_settings.AzureSpeechRegion}.api.cognitive.microsoft.com/sts/v1.0/issueToken";

        using var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.AzureSpeechSubscriptionKey);

        _logger.LogInformation("Fetching new access token from {Endpoint}", tokenEndpoint);

        var response = await client.PostAsync(tokenEndpoint, new StringContent(string.Empty));
        response.EnsureSuccessStatusCode();

        _accessToken = await response.Content.ReadAsStringAsync();
        _tokenExpiry = DateTime.UtcNow.AddMinutes(9);

        _logger.LogInformation("Access token obtained successfully");
        return _accessToken;
    }

    /// <inheritdoc />
    public async Task<byte[]> SynthesizeSpeechAsync(string text)
    {
        if (!_configValidator.IsValid(_settings))
        {
            var error = _configValidator.GetValidationError(_settings);
            _logger.LogError("Azure Speech settings validation failed: {Error}", error);
            throw new InvalidOperationException($"Azure Speech settings are not configured: {error}");
        }

        _logger.LogInformation("Starting speech synthesis for text length: {Length}", text.Length);

        var accessToken = await GetAccessTokenAsync();
        var ttsEndpoint = $"https://{_settings.AzureSpeechRegion}.tts.speech.microsoft.com/cognitiveservices/v1";

        var ssml = $"""
            <speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-GB'>
                <voice name='{VoiceName}'>
                    {System.Security.SecurityElement.Escape(text)}
                </voice>
            </speak>
            """;

        using var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, ttsEndpoint);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("X-Microsoft-OutputFormat", "audio-16khz-32kbitrate-mono-mp3");
        request.Headers.Add("User-Agent", "PoVicTranslate");
        request.Content = new StringContent(ssml, Encoding.UTF8, "application/ssml+xml");

        _logger.LogInformation("Sending TTS request to {Endpoint}", ttsEndpoint);

        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Speech synthesis failed: {StatusCode} - {Error}", response.StatusCode, errorContent);

            var errorMessage = response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => "Authentication failed. Verify your Azure Speech Service subscription key.",
                System.Net.HttpStatusCode.BadRequest => "Bad request. The voice or configuration may not be supported.",
                System.Net.HttpStatusCode.TooManyRequests => "Too many requests. Please try again later.",
                System.Net.HttpStatusCode.Forbidden => "Access forbidden. Check your subscription permissions.",
                _ => $"Speech synthesis failed: {response.StatusCode}. Details: {errorContent}"
            };

            throw new InvalidOperationException(errorMessage);
        }

        var audioData = await response.Content.ReadAsByteArrayAsync();
        _logger.LogInformation("Speech synthesis completed. Audio size: {Size} bytes", audioData.Length);

        return audioData;
    }
}
