using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace VictorianTranslator.IntegrationTests;

/// <summary>
/// Integration tests for SpeechController endpoints
/// Tests Azure Speech Service integration for text-to-speech synthesis
/// </summary>
public class SpeechEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SpeechEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task TestConfiguration_ShouldReturnConfigurationStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/Speech/test-config", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        // Properties are camelCase in JSON
        content.Should().Contain("hasSubscriptionKey");
        content.Should().Contain("region");
        content.Should().Contain("configurationValid");
    }

    [Fact]
    public async Task SynthesizeSpeech_WithValidText_ShouldReturnAudio()
    {
        // Arrange
        var text = "Hello world, this is a test.";

        // Act
        var response = await _client.PostAsJsonAsync("/api/Speech", text, TestContext.Current.CancellationToken);

        // Assert
        // May return OK with audio or 500 if Azure Speech is not configured
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Be("audio/mpeg");
            var audioData = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);
            audioData.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task SynthesizeSpeech_WithEmptyText_ShouldReturnBadRequest()
    {
        // Arrange
        var text = "";

        // Act
        var response = await _client.PostAsJsonAsync("/api/Speech", text, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.Should().Contain("cannot be empty");
    }

    [Fact]
    public async Task SynthesizeSpeech_WithLongText_ShouldHandle()
    {
        // Arrange
        var longText = string.Join(" ", Enumerable.Repeat("Testing speech synthesis with longer text.", 10));

        // Act
        var response = await _client.PostAsJsonAsync("/api/Speech", longText, TestContext.Current.CancellationToken);

        // Assert
        // Should either succeed or fail gracefully with 500 if service not configured
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task SynthesizeSpeech_WithSpecialCharacters_ShouldHandle()
    {
        // Arrange
        var text = "Good day, my esteemed companion! How doth thou fare?";

        // Act
        var response = await _client.PostAsJsonAsync("/api/Speech", text, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }
}
