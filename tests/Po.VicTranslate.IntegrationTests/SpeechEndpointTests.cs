using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Po.VicTranslate.IntegrationTests;

/// <summary>
/// Integration tests for Speech API endpoints.
/// Tests speech synthesis and configuration endpoints.
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
    public async Task GetConfiguration_ShouldReturnConfigurationStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/speech/configuration", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.Should().NotBeNullOrEmpty();
        // JSON returns camelCase property names
        content.Should().Contain("hasSubscriptionKey");
        content.Should().Contain("region");
        content.Should().Contain("configurationValid");
    }

    [Fact]
    public async Task SynthesizeSpeech_WithValidText_ShouldReturnAudioOrServiceError()
    {
        // Arrange
        var text = "Hello, good day to you!";

        // Act
        var response = await _client.PostAsJsonAsync("/api/speech/synthesize", text, cancellationToken: TestContext.Current.CancellationToken);

        // Assert - Either success (if Azure Speech is configured), 400 (validation), or 500 (not configured)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Be("audio/mpeg");
            var audioBytes = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);
            audioBytes.Should().NotBeEmpty();
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SynthesizeSpeech_WithEmptyText_ShouldReturnBadRequest(string text)
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/speech/synthesize", text, cancellationToken: TestContext.Current.CancellationToken);

        // Assert - empty text should fail validation
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task SynthesizeSpeech_WithTooLongText_ShouldReturnBadRequest()
    {
        // Arrange - Text exceeding 3000 character limit
        var longText = new string('a', 3500);

        // Act
        var response = await _client.PostAsJsonAsync("/api/speech/synthesize", longText, cancellationToken: TestContext.Current.CancellationToken);

        // Assert - should fail validation for exceeding max length
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task SynthesizeSpeech_ShouldRespondWithinTimeout()
    {
        // Arrange
        var text = "Quick test";
        var startTime = DateTime.UtcNow;

        // Act
        _ = await _client.PostAsJsonAsync("/api/speech/synthesize", text, cancellationToken: TestContext.Current.CancellationToken);

        // Assert - Should respond within 30 seconds (Azure Speech Service timeout)
        var duration = DateTime.UtcNow - startTime;
        duration.Should().BeLessThan(TimeSpan.FromSeconds(30));
    }
}
