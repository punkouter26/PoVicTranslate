using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Po.VicTranslate.Api.Controllers;
using Po.VicTranslate.Api.Services;
using Xunit;

namespace Po.VicTranslate.UnitTests.Controllers;

public class LyricsControllerTests
{
    private readonly Mock<ILyricsService> _mockLyricsService;
    private readonly LyricsController _controller;

    public LyricsControllerTests()
    {
        _mockLyricsService = new Mock<ILyricsService>();
        _controller = new LyricsController(_mockLyricsService.Object);
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
        var result = await _controller.GetAvailableSongs();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(songIds);
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
        var result = await _controller.GetLyrics(songFileName);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().Be(lyrics);
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
        var result = await _controller.GetLyrics(songFileName);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

}
