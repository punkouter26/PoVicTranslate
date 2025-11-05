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
    private readonly Mock<ILyricsManagementService> _mockManagementService;
    private readonly Mock<ILyricsUtilityService> _mockLyricsUtility;
    private readonly LyricsController _controller;

    public LyricsControllerTests()
    {
        _mockLyricsService = new Mock<ILyricsService>();
        _mockManagementService = new Mock<ILyricsManagementService>();
        _mockLyricsUtility = new Mock<ILyricsUtilityService>();
        
        // Setup default behavior for LimitWords to pass through
        _mockLyricsUtility
            .Setup(x => x.LimitWords(It.IsAny<string>(), It.IsAny<int>()))
            .Returns<string, int>((text, maxWords) => text);
        
        _controller = new LyricsController(
            _mockLyricsService.Object,
            _mockManagementService.Object,
            _mockLyricsUtility.Object);
    }

    [Fact]
    public async Task GetAvailableSongs_ShouldReturnSongList()
    {
        // Arrange
        var collection = new LyricsCollection
        {
            Version = "1.0",
            Songs = new List<Song>
            {
                new() { Id = "1", Title = "Song 1", Artist = "Artist 1", Album = "Album 1", Genre = "Rock", Content = "Lyrics 1" },
                new() { Id = "2", Title = "Song 2", Artist = "Artist 2", Album = "Album 2", Genre = "Pop", Content = "Lyrics 2" }
            }
        };

        _mockManagementService
            .Setup(x => x.LoadLyricsCollectionAsync())
            .ReturnsAsync(collection);

        // Act
        var result = await _controller.GetAvailableSongs();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetLyrics_WithValidId_ShouldReturnLyrics()
    {
        // Arrange
        const string songId = "test-song";
        var song = new Song
        {
            Id = songId,
            Title = "Test Song",
            Artist = "Test Artist",
            Album = "Test Album",
            Genre = "Test",
            Content = "Test lyrics content"
        };

        _mockManagementService
            .Setup(x => x.GetSongByIdAsync(songId))
            .ReturnsAsync(song);

        // Act
        var result = await _controller.GetLyrics(songId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().Be("Test lyrics content");
    }

    [Fact]
    public async Task GetLyrics_WithInvalidId_FallsBackToOldService()
    {
        // Arrange
        const string songId = "invalid-song";
        const string fallbackLyrics = "Fallback lyrics";

        _mockManagementService
            .Setup(x => x.GetSongByIdAsync(songId))
            .ReturnsAsync((Song?)null);

        _mockLyricsService
            .Setup(x => x.GetLyricsAsync(songId))
            .ReturnsAsync(fallbackLyrics);

        // Act
        var result = await _controller.GetLyrics(songId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().Be(fallbackLyrics);
    }

    [Fact]
    public async Task GetLyrics_WhenBothFail_ShouldReturnNotFound()
    {
        // Arrange
        const string songId = "missing-song";

        _mockManagementService
            .Setup(x => x.GetSongByIdAsync(songId))
            .ReturnsAsync((Song?)null);

        _mockLyricsService
            .Setup(x => x.GetLyricsAsync(songId))
            .ReturnsAsync((string)null!);

        // Act
        var result = await _controller.GetLyrics(songId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

}
