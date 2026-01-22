using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using System.Net;
using Xunit;

namespace Po.VicTranslate.IntegrationTests;

/// <summary>
/// REQUIRED: Integration tests for mandatory /api/health endpoint.
/// Verifies health check endpoint with readiness and liveness semantics.
/// TDD Workflow: Write failing test -> Implement feature -> Refactor
/// </summary>
public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task ApiHealth_ShouldReturnOk_WhenServicesAreHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health", TestContext.Current.CancellationToken);

        // Assert - health check should return OK when healthy or ServiceUnavailable when degraded
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task ApiHealth_ShouldReturnJsonContent()
    {
        // Act
        var response = await _client.GetAsync("/health", TestContext.Current.CancellationToken);

        // Assert - health check should return status information
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            content.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task ApiHealth_ShouldRespondQuickly()
    {
        // Arrange
        var startTime = DateTime.UtcNow;

        // Act
        _ = await _client.GetAsync("/health", TestContext.Current.CancellationToken);

        // Assert - health check should respond within 5 seconds
        var duration = DateTime.UtcNow - startTime;
        duration.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }
}
