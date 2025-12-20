using FluentAssertions;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using Po.VicTranslate.Client.Models;
using Po.VicTranslate.Client.Services;
using System.Text.Json;
using Xunit;

namespace Po.VicTranslate.UnitTests.Services;

public class HistoryServiceTests
{
    private readonly Mock<IJSRuntime> _jsRuntimeMock;
    private readonly HistoryService _service;

    public HistoryServiceTests()
    {
        _jsRuntimeMock = new Mock<IJSRuntime>();
        _service = new HistoryService(_jsRuntimeMock.Object);
    }

    #region GetHistoryAsync - Loading

    [Fact]
    public async Task GetHistoryAsync_WhenStorageIsEmpty_ReturnsEmptyList()
    {
        // Arrange
        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _service.GetHistoryAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHistoryAsync_LoadsFromLocalStorage()
    {
        // Arrange
        var items = new List<TranslationHistoryItem>
        {
            new() { InputText = "yo dawg", TranslatedText = "Good day, my esteemed companion", Timestamp = DateTime.Now }
        };
        var json = JsonSerializer.Serialize(items);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(json);

        // Act
        var result = await _service.GetHistoryAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].InputText.Should().Be("yo dawg");
    }

    [Fact]
    public async Task GetHistoryAsync_OnlyLoadsFromStorageOnce()
    {
        // Arrange
        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        // Act
        await _service.GetHistoryAsync();
        await _service.GetHistoryAsync();
        await _service.GetHistoryAsync();

        // Assert
        _jsRuntimeMock.Verify(
            x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task GetHistoryAsync_FiltersOutOldItems()
    {
        // Arrange
        var items = new List<TranslationHistoryItem>
        {
            new() { InputText = "recent", Timestamp = DateTime.Now.AddDays(-1) },
            new() { InputText = "old", Timestamp = DateTime.Now.AddDays(-10) }, // Older than 7 days
            new() { InputText = "very old", Timestamp = DateTime.Now.AddDays(-30) }
        };
        var json = JsonSerializer.Serialize(items);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(json);

        // Act
        var result = await _service.GetHistoryAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].InputText.Should().Be("recent");
    }

    [Fact]
    public async Task GetHistoryAsync_WhenFilteringOldItems_SavesCleanedList()
    {
        // Arrange
        var items = new List<TranslationHistoryItem>
        {
            new() { InputText = "recent", Timestamp = DateTime.Now },
            new() { InputText = "old", Timestamp = DateTime.Now.AddDays(-10) }
        };
        var json = JsonSerializer.Serialize(items);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(json);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.IsAny<object[]>()))
            .ReturnsAsync(Mock.Of<IJSVoidResult>());

        // Act
        await _service.GetHistoryAsync();

        // Assert
        _jsRuntimeMock.Verify(
            x => x.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task GetHistoryAsync_WhenStorageThrowsException_ReturnsEmptyList()
    {
        // Arrange
        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ThrowsAsync(new JSException("LocalStorage unavailable"));

        // Act
        var result = await _service.GetHistoryAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHistoryAsync_WhenJsonIsInvalid_ReturnsEmptyList()
    {
        // Arrange
        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync("invalid json {[}");

        // Act
        var result = await _service.GetHistoryAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region AddItemAsync

    [Fact]
    public async Task AddItemAsync_AddsNewItem()
    {
        // Arrange
        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        // Act
        await _service.AddItemAsync("yo", "Good day");

        // Assert
        var history = await _service.GetHistoryAsync();
        history.Should().HaveCount(1);
        history[0].InputText.Should().Be("yo");
        history[0].TranslatedText.Should().Be("Good day");
    }

    [Fact]
    public async Task AddItemAsync_AddsItemAtBeginning()
    {
        // Arrange
        var existingItems = new List<TranslationHistoryItem>
        {
            new() { InputText = "first", TranslatedText = "First", Timestamp = DateTime.Now }
        };
        var json = JsonSerializer.Serialize(existingItems);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(json);

        // Act
        await _service.AddItemAsync("second", "Second");

        // Assert
        var history = await _service.GetHistoryAsync();
        history[0].InputText.Should().Be("second"); // Most recent first
        history[1].InputText.Should().Be("first");
    }

    [Fact]
    public async Task AddItemAsync_RemovesDuplicateEntries()
    {
        // Arrange
        var existingItems = new List<TranslationHistoryItem>
        {
            new() { InputText = "yo", TranslatedText = "Good day", Timestamp = DateTime.Now.AddHours(-1) }
        };
        var json = JsonSerializer.Serialize(existingItems);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(json);

        // Act
        await _service.AddItemAsync("yo", "Good day, sir"); // Same input, different translation

        // Assert
        var history = await _service.GetHistoryAsync();
        history.Should().HaveCount(1); // Old duplicate removed
        history[0].TranslatedText.Should().Be("Good day, sir"); // New translation kept
    }

    [Fact]
    public async Task AddItemAsync_EnforcesMaxHistoryLimit()
    {
        // Arrange
        var existingItems = new List<TranslationHistoryItem>
        {
            new() { InputText = "item1", TranslatedText = "Item 1", Timestamp = DateTime.Now },
            new() { InputText = "item2", TranslatedText = "Item 2", Timestamp = DateTime.Now },
            new() { InputText = "item3", TranslatedText = "Item 3", Timestamp = DateTime.Now },
            new() { InputText = "item4", TranslatedText = "Item 4", Timestamp = DateTime.Now },
            new() { InputText = "item5", TranslatedText = "Item 5", Timestamp = DateTime.Now }
        };
        var json = JsonSerializer.Serialize(existingItems);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(json);

        // Act - Adding 6th item should remove oldest
        await _service.AddItemAsync("item6", "Item 6");

        // Assert
        var history = await _service.GetHistoryAsync();
        history.Should().HaveCount(5); // Max is 5
        history[0].InputText.Should().Be("item6"); // Newest first
        history.Should().NotContain(h => h.InputText == "item5"); // Oldest removed
    }

    [Fact]
    public async Task AddItemAsync_SavesToLocalStorage()
    {
        // Arrange
        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        string? savedJson = null;
        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.IsAny<object[]>()))
            .Callback<string, object[]>((method, args) =>
            {
                if (args.Length >= 2)
                {
                    savedJson = args[1] as string;
                }
            })
            .ReturnsAsync(Mock.Of<IJSVoidResult>());

        // Act
        await _service.AddItemAsync("test", "Test translation");

        // Assert
        savedJson.Should().NotBeNullOrEmpty();
        savedJson.Should().Contain("test");
        savedJson.Should().Contain("Test translation");
    }

    [Fact]
    public async Task AddItemAsync_SetsTimestamp()
    {
        // Arrange
        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        var before = DateTime.Now.AddSeconds(-1);

        // Act
        await _service.AddItemAsync("test", "Test");

        // Assert
        var history = await _service.GetHistoryAsync();
        var after = DateTime.Now.AddSeconds(1);
        history[0].Timestamp.Should().BeAfter(before);
        history[0].Timestamp.Should().BeBefore(after);
    }

    #endregion

    #region RemoveItemAsync

    [Fact]
    public async Task RemoveItemAsync_RemovesItemById()
    {
        // Arrange
        var items = new List<TranslationHistoryItem>
        {
            new() { Id = "id1", InputText = "first", TranslatedText = "First", Timestamp = DateTime.Now },
            new() { Id = "id2", InputText = "second", TranslatedText = "Second", Timestamp = DateTime.Now }
        };
        var json = JsonSerializer.Serialize(items);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(json);

        // Act
        await _service.RemoveItemAsync("id1");

        // Assert
        var history = await _service.GetHistoryAsync();
        history.Should().HaveCount(1);
        history[0].Id.Should().Be("id2");
    }

    [Fact]
    public async Task RemoveItemAsync_WhenItemNotFound_DoesNotThrow()
    {
        // Arrange
        var items = new List<TranslationHistoryItem>
        {
            new() { Id = "id1", InputText = "first", TranslatedText = "First", Timestamp = DateTime.Now }
        };
        var json = JsonSerializer.Serialize(items);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(json);

        // Act
        var act = async () => await _service.RemoveItemAsync("nonexistent");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RemoveItemAsync_SavesAfterRemoval()
    {
        // Arrange
        var items = new List<TranslationHistoryItem>
        {
            new() { Id = "id1", InputText = "test", TranslatedText = "Test", Timestamp = DateTime.Now }
        };
        var json = JsonSerializer.Serialize(items);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(json);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.IsAny<object[]>()))
            .ReturnsAsync(Mock.Of<IJSVoidResult>());

        // Act
        await _service.RemoveItemAsync("id1");

        // Assert
        _jsRuntimeMock.Verify(
            x => x.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.IsAny<object[]>()),
            Times.Once);
    }

    #endregion

    #region ClearHistoryAsync

    [Fact]
    public async Task ClearHistoryAsync_RemovesAllItems()
    {
        // Arrange
        var items = new List<TranslationHistoryItem>
        {
            new() { InputText = "first", TranslatedText = "First", Timestamp = DateTime.Now },
            new() { InputText = "second", TranslatedText = "Second", Timestamp = DateTime.Now }
        };
        var json = JsonSerializer.Serialize(items);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(json);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.IsAny<object[]>()))
            .ReturnsAsync(Mock.Of<IJSVoidResult>());

        // Act - Load first to populate cache
        await _service.GetHistoryAsync();
        await _service.ClearHistoryAsync();

        // Assert
        var history = await _service.GetHistoryAsync();
        history.Should().BeEmpty();
    }

    [Fact]
    public async Task ClearHistoryAsync_SavesEmptyList()
    {
        // Arrange
        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        string? savedJson = null;
        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.IsAny<object[]>()))
            .Callback<string, object[]>((method, args) =>
            {
                if (args.Length >= 2)
                {
                    savedJson = args[1] as string;
                }
            })
            .ReturnsAsync(Mock.Of<IJSVoidResult>());

        // Act
        await _service.ClearHistoryAsync();

        // Assert
        savedJson.Should().Be("[]");
    }

    #endregion

    #region Save Error Handling

    [Fact]
    public async Task AddItemAsync_WhenSaveThrowsException_DoesNotThrow()
    {
        // Arrange
        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.IsAny<object[]>()))
            .ThrowsAsync(new JSException("LocalStorage is full"));

        // Act
        var act = async () => await _service.AddItemAsync("test", "Test");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RemoveItemAsync_WhenSaveThrowsException_DoesNotThrow()
    {
        // Arrange
        var items = new List<TranslationHistoryItem>
        {
            new() { Id = "id1", InputText = "test", TranslatedText = "Test", Timestamp = DateTime.Now }
        };
        var json = JsonSerializer.Serialize(items);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(json);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.IsAny<object[]>()))
            .ThrowsAsync(new JSException("LocalStorage unavailable"));

        // Act
        var act = async () => await _service.RemoveItemAsync("id1");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ClearHistoryAsync_WhenSaveThrowsException_DoesNotThrow()
    {
        // Arrange
        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.IsAny<object[]>()))
            .ThrowsAsync(new JSException("LocalStorage unavailable"));

        // Act
        var act = async () => await _service.ClearHistoryAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Integration Scenarios

    [Fact]
    public async Task FullWorkflow_AddMultipleItems_GetHistory_RemoveOne_Clear()
    {
        // Arrange
        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        // Act & Assert - Add items
        await _service.AddItemAsync("yo", "Good day");
        await _service.AddItemAsync("what's up", "How do you do");

        var history1 = await _service.GetHistoryAsync();
        history1.Should().HaveCount(2);

        // Remove one item
        var itemToRemove = history1[1];
        await _service.RemoveItemAsync(itemToRemove.Id);

        var history2 = await _service.GetHistoryAsync();
        history2.Should().HaveCount(1);

        // Clear all
        await _service.ClearHistoryAsync();

        var history3 = await _service.GetHistoryAsync();
        history3.Should().BeEmpty();
    }

    [Fact]
    public async Task MultipleServices_ShareSameStorage()
    {
        // Arrange
        var sharedJson = (string?)null;

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(() => sharedJson);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.IsAny<object[]>()))
            .Callback<string, object[]>((method, args) =>
            {
                if (args.Length >= 2)
                {
                    sharedJson = args[1] as string;
                }
            })
            .ReturnsAsync(Mock.Of<IJSVoidResult>());

        var service1 = new HistoryService(_jsRuntimeMock.Object);
        var service2 = new HistoryService(_jsRuntimeMock.Object);

        // Act
        await service1.AddItemAsync("from service 1", "Translation 1");

        // Service 2 should see the item added by service 1
        var history = await service2.GetHistoryAsync();

        // Assert
        history.Should().HaveCount(1);
        history[0].InputText.Should().Be("from service 1");
    }

    #endregion
}
