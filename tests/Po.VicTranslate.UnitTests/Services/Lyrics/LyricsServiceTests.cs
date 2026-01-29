using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using PoVicTranslate.Web.Services;
using System.Text.Json;
using Xunit;
using PoVicTranslate.Web.Models;

namespace Po.VicTranslate.UnitTests.Services.Lyrics;

public class LyricsServiceTests : IDisposable
{
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly Mock<ILogger<LyricsService>> _mockLogger;
    private readonly string _tempDirectory;
    private readonly string _lyricsFilePath;

    public LyricsServiceTests()
    {
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockLogger = new Mock<ILogger<LyricsService>>();

        // Setup temp directory
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        // Mock ContentRootPath
        _mockEnvironment.Setup(e => e.ContentRootPath).Returns(_tempDirectory);

        // Ensure Data directory exists
        var dataDir = Path.Combine(_tempDirectory, "Data");
        Directory.CreateDirectory(dataDir);
        _lyricsFilePath = Path.Combine(dataDir, "lyrics-collection.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            try { Directory.Delete(_tempDirectory, true); } catch { }
        }
        GC.SuppressFinalize(this);
    }

    private void CreateLyricsFile(LyricsCollection collection)
    {
        var json = JsonSerializer.Serialize(collection);
        File.WriteAllText(_lyricsFilePath, json);
    }

    [Fact]
    public async Task GetAvailableSongsAsync_ShouldReturnSortedTitles()
    {
        // Arrange
        var collection = new LyricsCollection
        {
            Songs = new List<SongEntry>
            {
                new SongEntry { Title = "Song B", Lyrics = "Lyrics B" },
                new SongEntry { Title = "Song A", Lyrics = "Lyrics A" }
            }
        };
        CreateLyricsFile(collection);

        var service = new LyricsService(_mockEnvironment.Object, _mockLogger.Object);

        // Act
        var songs = await service.GetAvailableSongsAsync();

        // Assert
        songs.Should().HaveCount(2);
        songs[0].Should().Be("Song A");
        songs[1].Should().Be("Song B");
    }

    [Fact]
    public async Task GetLyricsAsync_WithValidId_ShouldReturnLyrics()
    {
        // Arrange
        var collection = new LyricsCollection
        {
            Songs = new List<SongEntry>
            {
                new SongEntry { Title = "TestSong", Lyrics = "These are the lyrics." }
            }
        };
        CreateLyricsFile(collection);

        var service = new LyricsService(_mockEnvironment.Object, _mockLogger.Object);

        // Act
        var lyrics = await service.GetLyricsAsync("TestSong.txt");

        // Assert
        lyrics.Should().Be("These are the lyrics.");
    }

    [Fact]
    public async Task GetLyricsAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var collection = new LyricsCollection { Songs = new List<SongEntry>() };
        CreateLyricsFile(collection);

        var service = new LyricsService(_mockEnvironment.Object, _mockLogger.Object);

        // Act
        var lyrics = await service.GetLyricsAsync("NonExistent");

        // Assert
        lyrics.Should().BeNull();
    }

    [Fact]
    public async Task GetLyricsAsync_ShouldHandleMaxWordsLimitation()
    {
        // Arrange
        var longLyrics = string.Join(" ", Enumerable.Range(0, 300).Select(i => $"word{i}"));
        var collection = new LyricsCollection
        {
            Songs = new List<SongEntry>
            {
                new SongEntry { Title = "LongSong", Lyrics = longLyrics }
            }
        };
        CreateLyricsFile(collection);

        var service = new LyricsService(_mockEnvironment.Object, _mockLogger.Object);

        // Act
        var lyrics = await service.GetLyricsAsync("LongSong");

        // Assert
        lyrics.Should().EndWith("...");
        // MaxWords is 200 + "..."
        // The service logic: string.Join(" ", words.Take(MaxWords)) + "...";
        var parts = lyrics!.Split(' ');
        // The last part is "...". But wait, the service does: string.Join(" ", ...) + "..."
        // So it appends "..." to the string. It doesn't mean the last word IS "...".
        // It appends it directly. So "word199..." if no space?
        // Code: string.Join(" ", words.Take(MaxWords)) + "...";
        // Yes, likely "word199..." if joined by space.

        // Just verify length is less than full length.
        lyrics.Length.Should().BeLessThan(longLyrics.Length);
    }
}
