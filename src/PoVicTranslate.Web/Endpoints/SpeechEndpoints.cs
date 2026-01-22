using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using PoVicTranslate.Web.Configuration;
using PoVicTranslate.Web.Services;
using PoVicTranslate.Web.Services.Validation;

namespace PoVicTranslate.Web.Endpoints;

/// <summary>
/// Extension methods for mapping speech API endpoints.
/// </summary>
public static class SpeechEndpoints
{
    /// <summary>
    /// Maps the speech API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapSpeechEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/speech")
            .WithTags("Speech")
            .WithOpenApi()
            .DisableAntiforgery();

        group.MapGet("/configuration", GetConfiguration)
            .WithName("GetSpeechConfiguration")
            .WithSummary("Gets the Azure Speech Service configuration status")
            .WithDescription("Returns configuration status including whether key and region are set.");

        group.MapPost("/synthesize", SynthesizeSpeechAsync)
            .WithName("SynthesizeSpeech")
            .WithSummary("Synthesizes speech audio from text")
            .WithDescription("Converts text to speech using Azure Cognitive Services and returns MP3 audio.");

        return app;
    }

    private static Ok<SpeechConfigurationStatus> GetConfiguration(IOptions<ApiSettings> apiSettings)
    {
        var settings = apiSettings.Value;
        var hasKey = !string.IsNullOrWhiteSpace(settings.AzureSpeechSubscriptionKey);
        var hasRegion = !string.IsNullOrWhiteSpace(settings.AzureSpeechRegion);

        var status = new SpeechConfigurationStatus(
            HasSubscriptionKey: hasKey,
            KeyPrefix: hasKey ? settings.AzureSpeechSubscriptionKey[..Math.Min(8, settings.AzureSpeechSubscriptionKey.Length)] + "..." : "MISSING",
            Region: settings.AzureSpeechRegion ?? "MISSING",
            ConfigurationValid: hasKey && hasRegion);

        return TypedResults.Ok(status);
    }

    private static async Task<Results<FileContentHttpResult, BadRequest<ValidationError>, StatusCodeHttpResult>> SynthesizeSpeechAsync(
        string text,
        IAudioSynthesisService audioSynthesisService,
        IInputValidator inputValidator,
        ILogger<AudioSynthesisService> logger)
    {
        // Validate and sanitize text input
        var textValidation = inputValidator.ValidateTextContent(text, maxLength: 3000);
        if (!textValidation.IsValid)
        {
            logger.LogWarning("Speech synthesis rejected: {Errors}", string.Join(", ", textValidation.Errors));
            return TypedResults.BadRequest(new ValidationError(textValidation.Errors));
        }

        var sanitizedText = textValidation.SanitizedValue!;

        try
        {
            logger.LogInformation("Synthesizing speech for text: '{Text}'", sanitizedText);
            var audioBytes = await audioSynthesisService.SynthesizeSpeechAsync(sanitizedText);
            logger.LogInformation("Speech synthesis successful. Audio size: {Size} bytes", audioBytes.Length);

            return TypedResults.File(audioBytes, "audio/mpeg", "speech.mp3");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Speech service not configured");
            return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error synthesizing speech: {Message}", ex.Message);
            return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}

/// <summary>
/// Speech service configuration status.
/// </summary>
public sealed record SpeechConfigurationStatus(
    bool HasSubscriptionKey,
    string KeyPrefix,
    string Region,
    bool ConfigurationValid);
