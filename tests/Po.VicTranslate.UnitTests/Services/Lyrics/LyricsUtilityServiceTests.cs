using Xunit;
using FluentAssertions;
using Po.VicTranslate.Api.Services.Lyrics;

namespace Po.VicTranslate.UnitTests.Services.Lyrics;

public class LyricsUtilityServiceTests
{
    private readonly LyricsUtilityService _service;

    public LyricsUtilityServiceTests()
    {
        _service = new LyricsUtilityService();
    }

    [Fact]
    public void LimitWords_WithTextShorterThanLimit_ReturnsOriginalText()
    {
        // Arrange
        const string text = "Hello world this is a test";
        const int maxWords = 10;

        // Act
        var result = _service.LimitWords(text, maxWords);

        // Assert
        result.Should().Be(text);
    }

    [Fact]
    public void LimitWords_WithTextLongerThanLimit_TruncatesAndAddsEllipsis()
    {
        // Arrange
        const string text = "One two three four five six seven eight nine ten";
        const int maxWords = 5;

        // Act
        var result = _service.LimitWords(text, maxWords);

        // Assert
        result.Should().Be("One two three four five...");
    }

    [Fact]
    public void LimitWords_WithExactlyMaxWords_ReturnsOriginalText()
    {
        // Arrange
        const string text = "One two three four five";
        const int maxWords = 5;

        // Act
        var result = _service.LimitWords(text, maxWords);

        // Assert
        result.Should().Be(text);
    }

    [Fact]
    public void LimitWords_WithNullText_ReturnsEmptyString()
    {
        // Act
        var result = _service.LimitWords(null!, 10);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void LimitWords_WithEmptyText_ReturnsEmptyString()
    {
        // Act
        var result = _service.LimitWords("", 10);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void LimitWords_WithWhitespaceText_ReturnsWhitespace()
    {
        // Arrange
        const string text = "   ";

        // Act
        var result = _service.LimitWords(text, 10);

        // Assert
        result.Should().Be(text);
    }

    [Fact]
    public void LimitWords_WithZeroMaxWords_ReturnsEmptyString()
    {
        // Arrange
        const string text = "Hello world";

        // Act
        var result = _service.LimitWords(text, 0);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void LimitWords_WithNegativeMaxWords_ReturnsEmptyString()
    {
        // Arrange
        const string text = "Hello world";

        // Act
        var result = _service.LimitWords(text, -5);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void LimitWords_WithMultilineText_SplitsCorrectly()
    {
        // Arrange
        const string text = "Line one\nLine two\nLine three\nLine four\nLine five";
        const int maxWords = 6;

        // Act
        var result = _service.LimitWords(text, maxWords);

        // Assert
        result.Should().Be("Line one Line two Line three...");
    }

    [Fact]
    public void LimitWords_WithMixedWhitespace_HandlesCorrectly()
    {
        // Arrange
        const string text = "Word1  Word2\n\nWord3\r\nWord4   Word5";
        const int maxWords = 3;

        // Act
        var result = _service.LimitWords(text, maxWords);

        // Assert
        result.Should().Be("Word1 Word2 Word3...");
    }
}
