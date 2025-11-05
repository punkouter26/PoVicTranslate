using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Po.VicTranslate.Api.Controllers;
using Xunit;

namespace VictorianTranslator.IntegrationTests;

/// <summary>
/// Integration tests for ClientLogController endpoints
/// Tests client-side log forwarding using Chain of Responsibility pattern
/// </summary>
public class ClientLogEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ClientLogEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task PostClientLog_WithInfoLevel_ShouldReturnOk()
    {
        // Arrange
        var logEntry = new ClientLogEntry
        {
            Level = "Info",
            Message = "Test info message from integration test",
            Page = "/test-page",
            Timestamp = DateTime.UtcNow,
            AdditionalData = new Dictionary<string, object>
            {
                { "testKey", "testValue" }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/log/client", logEntry, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.Should().Contain("Log received");
    }

    [Fact]
    public async Task PostClientLog_WithErrorLevel_ShouldReturnOk()
    {
        // Arrange
        var logEntry = new ClientLogEntry
        {
            Level = "Error",
            Message = "Test error message from integration test",
            Page = "/error-page",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/log/client", logEntry, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostClientLog_WithWarningLevel_ShouldReturnOk()
    {
        // Arrange
        var logEntry = new ClientLogEntry
        {
            Level = "Warning",
            Message = "Test warning message from integration test",
            Page = "/warning-page",
            UserAgent = "Mozilla/5.0 Test",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/log/client", logEntry, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostClientLog_WithUnknownLevel_ShouldReturnBadRequest()
    {
        // Arrange
        var logEntry = new ClientLogEntry
        {
            Level = "UnknownLevel",
            Message = "Test message with unknown level",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/log/client", logEntry, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostClientLog_WithCompleteData_ShouldReturnOk()
    {
        // Arrange
        var logEntry = new ClientLogEntry
        {
            Level = "Info",
            Message = "Complete log entry test",
            Page = "/complete-test",
            UserId = "test-user-123",
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
            Timestamp = DateTime.UtcNow,
            AdditionalData = new Dictionary<string, object>
            {
                { "component", "TestComponent" },
                { "action", "TestAction" },
                { "duration", 125 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/log/client", logEntry, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.Should().Contain("Log received");
    }
}
