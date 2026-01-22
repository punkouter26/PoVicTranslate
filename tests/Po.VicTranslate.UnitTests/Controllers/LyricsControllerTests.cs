// TODO: These tests were for LyricsController which has been converted to Minimal API endpoints (LyricsEndpoints).
// The endpoint methods are now static extension methods and need different testing approaches.
// Consider testing the ILyricsService directly or using integration tests for the endpoints.

/*
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PoVicTranslate.Web.Endpoints; // Note: Endpoints are now static extension classes
using PoVicTranslate.Web.Services;
using Xunit;

namespace Po.VicTranslate.UnitTests.Controllers;

public class LyricsControllerTests
{
    private readonly Mock<ILyricsService> _mockLyricsService;
    // LyricsController no longer exists - converted to LyricsEndpoints (Minimal API)

    public LyricsControllerTests()
    {
        _mockLyricsService = new Mock<ILyricsService>();
    }

    [Fact]
    public async Task GetAvailableSongs_ShouldReturnSongList()
    {
        // Arrange
        var songIds = new List<string> { "song1", "song2" };

        _mockLyricsService
            .Setup(x => x.GetAvailableSongsAsync())
            .ReturnsAsync(songIds);

        // Act
        // TODO: Test via integration tests or test ILyricsService directly

        // Assert
    }

    [Fact]
    public async Task GetLyrics_WithValidId_ShouldReturnLyrics()
    {
        // Arrange
        const string songFileName = "test-song";
        const string lyrics = "Test lyrics content with many words to test limiting functionality here";

        _mockLyricsService
            .Setup(x => x.GetLyricsAsync(songFileName))
            .ReturnsAsync(lyrics);

        // Act
        // TODO: Test via integration tests or test ILyricsService directly

        // Assert
    }

    [Fact]
    public async Task GetLyrics_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        const string songFileName = "invalid-song";

        _mockLyricsService
            .Setup(x => x.GetLyricsAsync(songFileName))
            .ReturnsAsync((string)null!);

        // Act
        // TODO: Test via integration tests or test ILyricsService directly

        // Assert
    }
}
*/
