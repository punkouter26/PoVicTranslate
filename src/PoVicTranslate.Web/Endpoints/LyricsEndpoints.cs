using Microsoft.AspNetCore.Http.HttpResults;
using PoVicTranslate.Web.Services;

namespace PoVicTranslate.Web.Endpoints;

/// <summary>
/// Extension methods for mapping lyrics API endpoints.
/// </summary>
public static class LyricsEndpoints
{
    /// <summary>
    /// Maps the lyrics API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapLyricsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/lyrics")
            .WithTags("Lyrics")
            .WithOpenApi()
            .DisableAntiforgery();

        group.MapGet("/titles", GetAvailableSongsAsync)
            .WithName("GetAvailableSongs")
            .WithSummary("Gets all available song titles")
            .WithDescription("Returns a list of all available song identifiers.");

        group.MapGet("/random", GetRandomLyricsAsync)
            .WithName("GetRandomLyrics")
            .WithSummary("Gets random lyrics")
            .WithDescription("Returns random lyrics from the collection.");

        group.MapGet("/{songTitle}", GetLyricsAsync)
            .WithName("GetLyrics")
            .WithSummary("Gets lyrics for a specific song")
            .WithDescription("Returns the lyrics for the specified song, limited to 200 words.");

        return app;
    }

    private static async Task<Ok<LyricsTitlesResponse>> GetAvailableSongsAsync(ILyricsService lyricsService)
    {
        var songs = await lyricsService.GetAvailableSongsAsync();
        return TypedResults.Ok(new LyricsTitlesResponse(songs));
    }

    private static async Task<Results<Ok<LyricsResponse>, NotFound>> GetLyricsAsync(
        string songTitle,
        ILyricsService lyricsService)
    {
        var lyrics = await lyricsService.GetLyricsAsync(songTitle);
        return lyrics is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(new LyricsResponse(songTitle, lyrics));
    }

    private static async Task<Results<Ok<LyricsResponse>, NotFound>> GetRandomLyricsAsync(ILyricsService lyricsService)
    {
        var (title, lyrics) = await lyricsService.GetRandomLyricsAsync();
        return lyrics is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(new LyricsResponse(title ?? "Random", lyrics));
    }
}

/// <summary>
/// Response DTO for lyrics titles endpoint.
/// </summary>
public sealed record LyricsTitlesResponse(List<string> Titles);

/// <summary>
/// Response DTO for lyrics content endpoint.
/// </summary>
public sealed record LyricsResponse(string Title, string Lyrics);
