using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Po.VicTranslate.Api.Services.Validation;
using Xunit;

namespace Po.VicTranslate.UnitTests.Services;

public class InputValidatorTests
{
    private readonly Mock<ILogger<InputValidator>> _mockLogger;
    private readonly InputValidator _validator;

    public InputValidatorTests()
    {
        _mockLogger = new Mock<ILogger<InputValidator>>();
        _validator = new InputValidator(_mockLogger.Object);
    }

    #region SearchQuery Tests

    [Fact]
    public void ValidateSearchQuery_WithNullQuery_ReturnsEmptyString()
    {
        // Act
        var result = _validator.ValidateSearchQuery(null);

        // Assert
        result.IsValid.Should().BeTrue();
        result.SanitizedValue.Should().Be(string.Empty);
    }

    [Fact]
    public void ValidateSearchQuery_WithEmptyQuery_ReturnsEmptyString()
    {
        // Act
        var result = _validator.ValidateSearchQuery("");

        // Assert
        result.IsValid.Should().BeTrue();
        result.SanitizedValue.Should().Be(string.Empty);
    }

    [Fact]
    public void ValidateSearchQuery_WithValidQuery_ReturnsSanitizedQuery()
    {
        // Arrange
        var query = "Victorian poetry";

        // Act
        var result = _validator.ValidateSearchQuery(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.SanitizedValue.Should().Be("Victorian poetry");
    }

    [Fact]
    public void ValidateSearchQuery_RemovesDangerousCharacters()
    {
        // Arrange
        var query = "search<script>alert('xss')</script>";

        // Act
        var result = _validator.ValidateSearchQuery(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.SanitizedValue.Should().NotContain("<");
        result.SanitizedValue.Should().NotContain(">");
        result.SanitizedValue.Should().NotContain("'");
        result.SanitizedValue.Should().Be("searchscriptalertxss/script");
    }

    [Fact]
    public void ValidateSearchQuery_WithExcessiveLength_ReturnsError()
    {
        // Arrange
        var query = new string('a', 201);

        // Act
        var result = _validator.ValidateSearchQuery(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("200 characters"));
    }

    [Fact]
    public void ValidateSearchQuery_RemovesExcessiveWhitespace()
    {
        // Arrange
        var query = "Victorian    poetry    search";

        // Act
        var result = _validator.ValidateSearchQuery(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.SanitizedValue.Should().Be("Victorian poetry search");
    }

    #endregion

    #region ResourceId Tests

    [Fact]
    public void ValidateResourceId_WithNullId_ReturnsError()
    {
        // Act
        var result = _validator.ValidateResourceId(null);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("cannot be empty"));
    }

    [Fact]
    public void ValidateResourceId_WithValidId_ReturnsId()
    {
        // Arrange
        var id = "song-123";

        // Act
        var result = _validator.ValidateResourceId(id);

        // Assert
        result.IsValid.Should().BeTrue();
        result.SanitizedValue.Should().Be("song-123");
    }

    [Fact]
    public void ValidateResourceId_WithInvalidCharacters_ReturnsError()
    {
        // Arrange
        var id = "song/../../../etc/passwd";

        // Act
        var result = _validator.ValidateResourceId(id);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Resource ID can only contain"));
    }

    [Fact]
    public void ValidateResourceId_WithExcessiveLength_ReturnsError()
    {
        // Arrange
        var id = new string('a', 101);

        // Act
        var result = _validator.ValidateResourceId(id);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("100 characters"));
    }

    [Theory]
    [InlineData("song123")]
    [InlineData("song_123")]
    [InlineData("song-123")]
    [InlineData("SONG123")]
    [InlineData("Song_123-ABC")]
    public void ValidateResourceId_WithValidFormats_Succeeds(string id)
    {
        // Act
        var result = _validator.ValidateResourceId(id);

        // Assert
        result.IsValid.Should().BeTrue();
        result.SanitizedValue.Should().Be(id);
    }

    #endregion

    #region TextContent Tests

    [Fact]
    public void ValidateTextContent_WithNullText_ReturnsError()
    {
        // Act
        var result = _validator.ValidateTextContent(null);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("cannot be empty"));
    }

    [Fact]
    public void ValidateTextContent_WithValidText_ReturnsText()
    {
        // Arrange
        var text = "Hello, good day to you!";

        // Act
        var result = _validator.ValidateTextContent(text);

        // Assert
        result.IsValid.Should().BeTrue();
        result.SanitizedValue.Should().Be(text);
    }

    [Fact]
    public void ValidateTextContent_ExceedingMaxLength_ReturnsError()
    {
        // Arrange
        var text = new string('a', 5001);

        // Act
        var result = _validator.ValidateTextContent(text, maxLength: 5000);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("5000 characters"));
    }

    [Fact]
    public void ValidateTextContent_WithNullBytes_ReturnsError()
    {
        // Arrange
        var text = "Hello\0World";

        // Act
        var result = _validator.ValidateTextContent(text);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("invalid characters"));
    }

    [Fact]
    public void ValidateTextContent_RemovesControlCharacters()
    {
        // Arrange
        var text = "Hello\u0001\u0002World\u0003!";

        // Act
        var result = _validator.ValidateTextContent(text);

        // Assert
        result.IsValid.Should().BeTrue();
        result.SanitizedValue.Should().Be("HelloWorld!");
    }

    [Fact]
    public void ValidateTextContent_PreservesCommonWhitespace()
    {
        // Arrange
        var text = "Hello\nWorld\tTest\r\nEnd";

        // Act
        var result = _validator.ValidateTextContent(text);

        // Assert
        result.IsValid.Should().BeTrue();
        result.SanitizedValue.Should().Contain("\n");
        result.SanitizedValue.Should().Contain("\t");
        result.SanitizedValue.Should().Contain("\r");
    }

    #endregion

    #region FilePath Tests

    [Fact]
    public void ValidateFilePath_WithNullPath_ReturnsError()
    {
        // Act
        var result = _validator.ValidateFilePath(null);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("cannot be empty"));
    }

    [Fact]
    public void ValidateFilePath_WithPathTraversal_ReturnsError()
    {
        // Arrange
        var path = "../../../etc/passwd";

        // Act
        var result = _validator.ValidateFilePath(path);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("invalid patterns"));
    }

    [Fact]
    public void ValidateFilePath_WithAbsolutePath_ReturnsError()
    {
        // Arrange
        var path = "C:\\Windows\\System32\\file.txt";

        // Act
        var result = _validator.ValidateFilePath(path);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("relative"));
    }

    [Fact]
    public void ValidateFilePath_WithValidRelativePath_Succeeds()
    {
        // Arrange
        var path = "data/lyrics/song.txt";

        // Act
        var result = _validator.ValidateFilePath(path);

        // Assert
        result.IsValid.Should().BeTrue();
        result.SanitizedValue.Should().Be(path);
    }

    [Fact]
    public void ValidateFilePath_WithAllowedExtension_Succeeds()
    {
        // Arrange
        var path = "data/lyrics/song.txt";
        var allowedExtensions = new[] { ".txt", ".json" };

        // Act
        var result = _validator.ValidateFilePath(path, allowedExtensions);

        // Assert
        result.IsValid.Should().BeTrue();
        result.SanitizedValue.Should().Be(path);
    }

    [Fact]
    public void ValidateFilePath_WithDisallowedExtension_ReturnsError()
    {
        // Arrange
        var path = "data/lyrics/song.exe";
        var allowedExtensions = new[] { ".txt", ".json" };

        // Act
        var result = _validator.ValidateFilePath(path, allowedExtensions);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("not allowed"));
    }

    #endregion

    #region NumericParameter Tests

    [Fact]
    public void ValidateNumericParameter_BelowMinimum_ReturnsError()
    {
        // Act
        var result = _validator.ValidateNumericParameter(0, min: 1, max: 100, parameterName: "pageSize");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("at least 1"));
    }

    [Fact]
    public void ValidateNumericParameter_AboveMaximum_ReturnsError()
    {
        // Act
        var result = _validator.ValidateNumericParameter(101, min: 1, max: 100, parameterName: "pageSize");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("cannot exceed 100"));
    }

    [Fact]
    public void ValidateNumericParameter_WithinRange_Succeeds()
    {
        // Act
        var result = _validator.ValidateNumericParameter(50, min: 1, max: 100, parameterName: "pageSize");

        // Assert
        result.IsValid.Should().BeTrue();
        result.Value.Should().Be(50);
    }

    [Fact]
    public void ValidateNumericParameter_AtBoundaries_Succeeds()
    {
        // Act
        var resultMin = _validator.ValidateNumericParameter(1, min: 1, max: 100, parameterName: "pageSize");
        var resultMax = _validator.ValidateNumericParameter(100, min: 1, max: 100, parameterName: "pageSize");

        // Assert
        resultMin.IsValid.Should().BeTrue();
        resultMin.Value.Should().Be(1);
        resultMax.IsValid.Should().BeTrue();
        resultMax.Value.Should().Be(100);
    }

    #endregion
}
