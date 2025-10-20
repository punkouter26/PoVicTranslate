using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using VictorianTranslator.Models;
using Xunit;

namespace VictorianTranslator.IntegrationTests;

public class TranslationEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TranslationEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Translate_WithValidText_ShouldReturnTranslation()
    {
        // Arrange
        var request = new TranslationRequest
        {
            Text = "Hello, how are you today?"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/Translation/translate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TranslationResponse>();
        result.Should().NotBeNull();
        result!.TranslatedText.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Translate_WithEmptyText_ShouldReturnBadRequest(string? text)
    {
        // Arrange
        var request = new TranslationRequest
        {
            Text = text!
        };

        // Act
        var response = await _client.PostAsJsonAsync("/Translation/translate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Translate_WithLongText_ShouldHandle()
    {
        // Arrange
        var longText = string.Join(" ", Enumerable.Repeat("This is a test sentence.", 100));
        var request = new TranslationRequest
        {
            Text = longText
        };

        // Act
        var response = await _client.PostAsJsonAsync("/Translation/translate", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.RequestTimeout);
    }
}
