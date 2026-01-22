using FluentAssertions;
using PoVicTranslate.Web.Services.Validation;
using Xunit;

namespace Po.VicTranslate.UnitTests.Services;

public class InputValidatorTests
{
    private readonly InputValidator _validator;

    public InputValidatorTests()
    {
        _validator = new InputValidator();
    }

    #region ValidateTextContent Tests

    [Fact]
    public void ValidateTextContent_WithNullText_ReturnsInvalid()
    {
        // Act
        var result = _validator.ValidateTextContent(null, 1000);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Text cannot be empty");
    }

    [Fact]
    public void ValidateTextContent_WithEmptyText_ReturnsInvalid()
    {
        // Act
        var result = _validator.ValidateTextContent("", 1000);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Text cannot be empty");
    }

    [Fact]
    public void ValidateTextContent_WithWhitespaceText_ReturnsInvalid()
    {
        // Act
        var result = _validator.ValidateTextContent("   ", 1000);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Text cannot be empty");
    }

    [Fact]
    public void ValidateTextContent_WithValidText_ReturnsValid()
    {
        // Arrange
        var text = "Hello, this is valid text content.";

        // Act
        var result = _validator.ValidateTextContent(text, 1000);

        // Assert
        result.IsValid.Should().BeTrue();
        result.SanitizedValue.Should().Be(text);
    }

    [Fact]
    public void ValidateTextContent_ExceedingMaxLength_ReturnsInvalid()
    {
        // Arrange
        var text = new string('a', 100);

        // Act
        var result = _validator.ValidateTextContent(text, 50);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Text exceeds maximum length of 50 characters");
    }

    [Fact]
    public void ValidateTextContent_AtExactMaxLength_ReturnsValid()
    {
        // Arrange
        var text = new string('a', 50);

        // Act
        var result = _validator.ValidateTextContent(text, 50);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateTextContent_WithScriptTag_ReturnsInvalid()
    {
        // Arrange
        var text = "Hello <script>alert('xss')</script> World";

        // Act
        var result = _validator.ValidateTextContent(text, 1000);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Text contains potentially malicious content");
    }

    [Fact]
    public void ValidateTextContent_WithJavascriptUri_ReturnsInvalid()
    {
        // Arrange
        var text = "Click here: javascript:void(0)";

        // Act
        var result = _validator.ValidateTextContent(text, 1000);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Text contains potentially malicious content");
    }

    [Fact]
    public void ValidateTextContent_RemovesControlCharacters()
    {
        // Arrange
        var text = "Hello\x00\x01\x02World";

        // Act
        var result = _validator.ValidateTextContent(text, 1000);

        // Assert
        result.IsValid.Should().BeTrue();
        result.SanitizedValue.Should().Be("HelloWorld");
    }

    [Fact]
    public void ValidateTextContent_PreservesNewlinesAndTabs()
    {
        // Arrange
        var text = "Hello\n\tWorld";

        // Act
        var result = _validator.ValidateTextContent(text, 1000);

        // Assert
        result.IsValid.Should().BeTrue();
        result.SanitizedValue.Should().Contain("\n");
        result.SanitizedValue.Should().Contain("\t");
    }

    [Fact]
    public void ValidateTextContent_TrimsWhitespace()
    {
        // Arrange
        var text = "  Hello World  ";

        // Act
        var result = _validator.ValidateTextContent(text, 1000);

        // Assert
        result.IsValid.Should().BeTrue();
        result.SanitizedValue.Should().Be("Hello World");
    }

    #endregion
}
