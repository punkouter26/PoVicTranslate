using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Po.VicTranslate.Api.Controllers;
using Xunit;

namespace VictorianTranslator.IntegrationTests;

/// <summary>
/// Integration tests for DebugController endpoints
/// Tests debug logging functionality using Strategy Pattern
/// </summary>
public class DebugEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public DebugEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetRecentLogs_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/Debug/logs?count=10", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRecentLogs_WithEventTypeFilter_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/Debug/logs?count=5&eventType=Test", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSummaryReport_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/Debug/summary-report", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.Should().NotBeNull();
    }

    [Fact]
    public async Task LogTestEvent_ShouldReturnOk()
    {
        // Arrange
        var request = new TestEventRequest
        {
            EventType = "IntegrationTest",
            Message = "Test event from integration tests",
            Data = new { testKey = "testValue" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Debug/test-event", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.Should().Contain("Test event logged successfully");
    }

    [Fact]
    public async Task LogTestInstability_ShouldReturnOk()
    {
        // Arrange
        var request = new TestInstabilityRequest
        {
            Component = "IntegrationTestComponent",
            Issue = "Simulated instability for testing",
            DiagnosticData = new { timestamp = DateTime.UtcNow }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Debug/test-instability", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.Should().Contain("Test instability logged successfully");
    }

    [Fact]
    public async Task LogTestFailure_ShouldReturnOk()
    {
        // Arrange
        var request = new TestFailureRequest
        {
            Component = "IntegrationTestComponent",
            Failure = "Simulated failure for testing",
            IncludeException = true,
            ExceptionMessage = "Test exception message",
            Context = new { testId = "integration-test-001" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Debug/test-failure", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.Should().Contain("Test structural failure logged successfully");
    }

    [Fact]
    public async Task CleanupOldLogs_ShouldReturnOk()
    {
        // Act
        var response = await _client.DeleteAsync("/api/Debug/cleanup?retentionHours=24", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.Should().Contain("Cleanup completed");
    }

    [Fact]
    public async Task ReceiveBrowserLog_WithEventType_ShouldReturnOk()
    {
        // Arrange
        var request = new BrowserLogRequest
        {
            Type = "event",
            Payload = new BrowserLogPayload
            {
                EventType = "ButtonClick",
                Message = "User clicked button from integration test",
                Component = "TestButton",
                SessionId = "test-session-123",
                Timestamp = DateTime.UtcNow.ToString("o"),
                Url = "/test-page"
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Debug/browser-log", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.Should().Contain("Browser log received successfully");
    }

    [Fact]
    public async Task ReceiveBrowserLog_WithInstabilityType_ShouldReturnOk()
    {
        // Arrange
        var request = new BrowserLogRequest
        {
            Type = "instability",
            Payload = new BrowserLogPayload
            {
                Component = "TestComponent",
                Issue = "Simulated instability from browser",
                DiagnosticData = new { memoryUsage = 512, cpuLoad = 75 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Debug/browser-log", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetDebugHealth_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/Debug/health", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.Should().Contain("status");
        content.Should().Contain("timestamp");
    }
}
