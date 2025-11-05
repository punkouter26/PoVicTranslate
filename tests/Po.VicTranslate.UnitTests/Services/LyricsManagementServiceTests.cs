using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Po.VicTranslate.Api.Models;
using Po.VicTranslate.Api.Services;
using Po.VicTranslate.Api.Services.Caching;
using Xunit;

namespace VictorianTranslator.UnitTests.Services;

public class LyricsManagementServiceTests : IDisposable
{
    private readonly Mock<ILogger<LyricsManagementService>> _mockLogger;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly string _tempDataPath;
    private readonly string _tempContentRoot;
    private LyricsManagementService? _service;

    public LyricsManagementServiceTests()
    {
        _mockLogger = new Mock<ILogger<LyricsManagementService>>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockCacheService = new Mock<ICacheService>();

        // Create a temp directory for testing
        _tempContentRoot = Path.Combine(Path.GetTempPath(), $"LyricsTests_{Guid.NewGuid()}");
        _tempDataPath = Path.Combine(_tempContentRoot, "Data");
        Directory.CreateDirectory(_tempDataPath);

        _mockEnvironment.Setup(x => x.ContentRootPath).Returns(_tempContentRoot);

        // Setup cache service to execute the factory function directly (pass-through behavior)
        _mockCacheService
            .Setup(x => x.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<LyricsCollection>>>(),
                It.IsAny<TimeSpan?>()))
            .Returns<string, Func<Task<LyricsCollection>>, TimeSpan?>(
                (key, factory, expiration) => factory());

        _mockCacheService
            .Setup(x => x.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<Song?>>>(),
                It.IsAny<TimeSpan?>()))
            .Returns<string, Func<Task<Song?>>, TimeSpan?>(
                (key, factory, expiration) => factory());

        _mockCacheService
            .Setup(x => x.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<List<string>>>>(),
                It.IsAny<TimeSpan?>()))
            .Returns<string, Func<Task<List<string>>>, TimeSpan?>(
                (key, factory, expiration) => factory());

        // Create test data file
        var testCollection = new LyricsCollection
        {
            Version = "1.0",
            GeneratedAt = DateTime.UtcNow,
            TotalSongs = 2,
            Artists = new Dictionary<string, string>
            {
                { "Artist 1", "Artist 1" },
                { "Artist 2", "Artist 2" }
            },
            Albums = new Dictionary<string, string>
            {
                { "Album 1", "Album 1" },
                { "Album 2", "Album 2" }
            },
            Songs = new List<Song>
            {
                new()
                {
                    Id = "song1",
                    Title = "Test Song 1",
                    Artist = "Artist 1",
                    Album = "Album 1",
                    Genre = "Rock",
                    Content = "Test lyrics content 1"
                },
                new()
                {
                    Id = "song2",
                    Title = "Test Song 2",
                    Artist = "Artist 2",
                    Album = "Album 2",
                    Genre = "Pop",
                    Content = "Test lyrics content 2"
                }
            }
        };

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        File.WriteAllText(
            Path.Combine(_tempDataPath, "lyrics-collection.json"),
            JsonSerializer.Serialize(testCollection, jsonOptions));

        _service = new LyricsManagementService(_mockLogger.Object, _mockEnvironment.Object, _mockCacheService.Object);
    }

    [Fact]
    public async Task LoadLyricsCollectionAsync_ShouldLoadFromFile()
    {
        // Act
        var result = await _service!.LoadLyricsCollectionAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalSongs.Should().Be(2);
        result.Songs.Should().HaveCount(2);
        result.Version.Should().Be("1.0");
    }

    [Fact]
    public async Task LoadLyricsCollectionAsync_ShouldUseCache()
    {
        // Act
        var result1 = await _service!.LoadLyricsCollectionAsync();
        var result2 = await _service.LoadLyricsCollectionAsync();

        // Assert - Verify cache service was called
        _mockCacheService.Verify(
            x => x.GetOrCreateAsync(
                It.Is<string>(key => key == "lyrics:collection"),
                It.IsAny<Func<Task<LyricsCollection>>>(),
                It.IsAny<TimeSpan?>()),
            Times.AtLeastOnce);
        
        // Both results should have the same data (even if different instances)
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.TotalSongs.Should().Be(result2.TotalSongs);
    }

    [Fact]
    public async Task GetSongByIdAsync_WithValidId_ShouldReturnSong()
    {
        // Act
        var result = await _service!.GetSongByIdAsync("song1");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("song1");
        result.Title.Should().Be("Test Song 1");
    }

    [Fact]
    public async Task GetSongByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _service!.GetSongByIdAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchLyricsAsync_WithEmptyQuery_ShouldReturnAllSongs()
    {
        // Act
        var result = await _service!.SearchLyricsAsync("");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchLyricsAsync_WithQuery_ShouldReturnMatchingSongs()
    {
        // Act
        var result = await _service!.SearchLyricsAsync("Song 1");

        // Assert
        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Test Song 1");
    }

    [Fact]
    public async Task GetAvailableArtistsAsync_ShouldReturnUniqueArtists()
    {
        // Act
        var result = await _service!.GetAvailableArtistsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain("Artist 1");
        result.Should().Contain("Artist 2");
    }

    [Fact]
    public async Task GetAvailableAlbumsAsync_ShouldReturnUniqueAlbums()
    {
        // Act
        var result = await _service!.GetAvailableAlbumsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain("Album 1");
        result.Should().Contain("Album 2");
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempContentRoot))
            {
                Directory.Delete(_tempContentRoot, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
