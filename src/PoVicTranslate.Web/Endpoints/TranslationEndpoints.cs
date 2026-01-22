using System.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using PoVicTranslate.Web.Models;
using PoVicTranslate.Web.Services;
using PoVicTranslate.Web.Services.Validation;

namespace PoVicTranslate.Web.Endpoints;

/// <summary>
/// Extension methods for mapping translation API endpoints.
/// </summary>
public static class TranslationEndpoints
{
    /// <summary>
    /// Maps the translation API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapTranslationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/translation")
            .WithTags("Translation")
            .WithOpenApi();

        group.MapPost("/", TranslateAsync)
            .WithName("Translate")
            .WithSummary("Translates modern English text to Victorian-era English")
            .WithDescription("Translates modern English text to Victorian-era English and generates audio.");

        return app;
    }

    private static async Task<Results<Ok<TranslationResponse>, BadRequest<ValidationError>>> TranslateAsync(
        TranslationRequest request,
        ITranslationService translationService,
        IAudioSynthesisService audioSynthesisService,
        ICustomTelemetryService telemetryService,
        IInputValidator inputValidator,
        ILogger<TranslationRequest> logger)
    {
        // Validate and sanitize text input
        var textValidation = inputValidator.ValidateTextContent(request.Text, maxLength: 5000);
        if (!textValidation.IsValid)
        {
            logger.LogWarning("Translation request rejected: {Errors}", string.Join(", ", textValidation.Errors));
            return TypedResults.BadRequest(new ValidationError(textValidation.Errors));
        }

        var sanitizedText = textValidation.SanitizedValue!;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("Processing translation request: TextLength={Length}", sanitizedText.Length);

        var translatedText = await translationService.TranslateToVictorianEnglishAsync(sanitizedText);

        // Synthesize speech (don't fail if this fails)
        byte[]? audioData = null;
        try
        {
            logger.LogInformation("Synthesizing speech for translated text");
            audioData = await audioSynthesisService.SynthesizeSpeechAsync(translatedText);
            logger.LogInformation("Speech synthesis successful, audio size: {Size} bytes", audioData.Length);
        }
        catch (Exception audioEx)
        {
            logger.LogWarning(audioEx, "Speech synthesis failed, but translation succeeded");
        }

        stopwatch.Stop();

        // Track telemetry
        telemetryService.TrackTranslationRequest(
            inputLanguage: "Modern English",
            textLength: sanitizedText.Length,
            success: true,
            durationMs: stopwatch.ElapsedMilliseconds);

        telemetryService.TrackUserActivity("Translation", userId: null);

        var response = new TranslationResponse(sanitizedText, translatedText, audioData);
        return TypedResults.Ok(response);
    }
}

/// <summary>
/// Represents a validation error response.
/// </summary>
public sealed record ValidationError(List<string> Errors);
