using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Po.VicTranslate.Api.Controllers;
using Po.VicTranslate.Api.Models;
using Po.VicTranslate.Api.Services;
using Po.VicTranslate.Api.Services.Lyrics;
using Xunit;

namespace VictorianTranslator.UnitTests.Controllers;

public class LyricsControllerTests
{
    private readonly Mock<ILyricsService> _mockLyricsService;
    private readonly Mock<ILyricsUtilityService> _mockLyricsUtility;
    private readonly LyricsController _controller;

    public LyricsControllerTests()
    {
        _mockLyricsService = new Mock<ILyricsService>();
        _mockLyricsUtility = new Mock<ILyricsUtilityService>();
        
        // Setup default behavior for LimitWords to pass through
        _mockLyricsUtility
            .Setup(x => x.LimitWords(It.IsAny<string>(), It.IsAny<int>()))
            .Returns<string, int>((text, maxWords) => text);
        
        _controller = new LyricsController(
            _mockLyricsService.Object,
            _mockLyricsUtility.Object);
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
        _mockLyricsUtility.Verify(x => x.LimitWords(lyrics, 200), Times.Once);
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
