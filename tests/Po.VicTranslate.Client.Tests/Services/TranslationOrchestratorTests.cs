using FluentAssertions;
using Po.VicTranslate.Client.ViewModels;
using Xunit;

namespace Po.VicTranslate.Client.Tests.Services;

/// <summary>
/// Unit tests for TranslationViewModel - the client-side view model.
/// Tests validation and state management logic that doesn't require HTTP dependencies.
/// </summary>
public class TranslationViewModelTests
{
    [Fact]
    public void TranslationViewModel_InitialState_ShouldHaveEmptyValues()
    {
        // Act
        var viewModel = new TranslationViewModel();

        // Assert
        viewModel.InputText.Should().BeEmpty();
        viewModel.TranslatedText.Should().BeEmpty();
        viewModel.ErrorMessage.Should().BeEmpty();
        viewModel.IsLoading.Should().BeFalse();
        viewModel.IsTranslating.Should().BeFalse();
        viewModel.AvailableSongs.Should().BeEmpty();
    }

    [Fact]
    public void WordCount_WithMultipleWords_ShouldReturnCorrectCount()
    {
        // Arrange
        var viewModel = new TranslationViewModel
        {
            InputText = "Hello world this is a test"
        };

        // Act & Assert
        viewModel.WordCount.Should().Be(6);
    }

    [Fact]
    public void WordCount_WithEmptyText_ShouldReturnZero()
    {
        // Arrange
        var viewModel = new TranslationViewModel
        {
            InputText = string.Empty
        };

        // Act & Assert
        viewModel.WordCount.Should().Be(0);
    }

    [Fact]
    public void WordCount_WithWhitespaceOnly_ShouldReturnZero()
    {
        // Arrange
        var viewModel = new TranslationViewModel
        {
            InputText = "   \t\n   "
        };

        // Act & Assert
        viewModel.WordCount.Should().Be(0);
    }

    [Fact]
    public void CanTranslate_WithValidText_ShouldReturnTrue()
    {
        // Arrange
        var viewModel = new TranslationViewModel
        {
            InputText = "Hello world",
            IsTranslating = false,
            IsLoading = false
        };

        // Act & Assert
        viewModel.CanTranslate.Should().BeTrue();
    }

    [Fact]
    public void CanTranslate_WhenTranslating_ShouldReturnFalse()
    {
        // Arrange
        var viewModel = new TranslationViewModel
        {
            InputText = "Hello world",
            IsTranslating = true
        };

        // Act & Assert
        viewModel.CanTranslate.Should().BeFalse();
    }

    [Fact]
    public void CanTranslate_WithEmptyText_ShouldReturnFalse()
    {
        // Arrange
        var viewModel = new TranslationViewModel
        {
            InputText = string.Empty
        };

        // Act & Assert
        viewModel.CanTranslate.Should().BeFalse();
    }

    [Fact]
    public void CanTranslate_WithTooManyWords_ShouldReturnFalse()
    {
        // Arrange
        var viewModel = new TranslationViewModel
        {
            InputText = string.Join(" ", Enumerable.Repeat("word", 250)) // 250 words > 200 limit
        };

        // Act & Assert
        viewModel.CanTranslate.Should().BeFalse();
    }

    [Fact]
    public void ClearError_ShouldSetErrorMessageToEmpty()
    {
        // Arrange
        var viewModel = new TranslationViewModel
        {
            ErrorMessage = "Some error occurred"
        };

        // Act
        viewModel.ClearError();

        // Assert
        viewModel.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public void ClearTranslation_ShouldClearTranslatedText()
    {
        // Arrange
        var viewModel = new TranslationViewModel
        {
            TranslatedText = "Some translated text"
        };

        // Act
        viewModel.ClearTranslation();

        // Assert
        viewModel.TranslatedText.Should().BeEmpty();
    }

    [Fact]
    public void EnterEditMode_ShouldSetIsEditModeAndCopyText()
    {
        // Arrange
        var viewModel = new TranslationViewModel
        {
            TranslatedText = "Original text"
        };

        // Act
        viewModel.EnterEditMode();

        // Assert
        viewModel.IsEditMode.Should().BeTrue();
        viewModel.EditedText.Should().Be("Original text");
    }

    [Fact]
    public void SaveEdit_ShouldUpdateTranslatedTextAndExitEditMode()
    {
        // Arrange
        var viewModel = new TranslationViewModel
        {
            TranslatedText = "Original text",
            IsEditMode = true,
            EditedText = "Edited text"
        };

        // Act
        viewModel.SaveEdit();

        // Assert
        viewModel.TranslatedText.Should().Be("Edited text");
        viewModel.IsEditMode.Should().BeFalse();
    }

    [Fact]
    public void CancelEdit_ShouldExitEditModeWithoutSaving()
    {
        // Arrange
        var viewModel = new TranslationViewModel
        {
            TranslatedText = "Original text"
        };
        // Enter edit mode first to set up OriginalTranslatedText
        viewModel.EnterEditMode();
        viewModel.EditedText = "Edited text";

        // Act
        viewModel.CancelEdit();

        // Assert
        viewModel.TranslatedText.Should().Be("Original text");
        viewModel.IsEditMode.Should().BeFalse();
        viewModel.EditedText.Should().BeEmpty();
    }

    [Fact]
    public void AvailableSongs_SetAndGet_ShouldWork()
    {
        // Arrange
        var viewModel = new TranslationViewModel();
        var songs = new List<string> { "song1.json", "song2.json" };

        // Act
        viewModel.AvailableSongs = songs;

        // Assert
        viewModel.AvailableSongs.Should().BeEquivalentTo(songs);
    }
}
