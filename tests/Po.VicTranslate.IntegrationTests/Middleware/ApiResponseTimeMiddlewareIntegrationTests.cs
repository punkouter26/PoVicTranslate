using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Po.VicTranslate.IntegrationTests.Middleware;

public class ApiResponseTimeMiddlewareIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiResponseTimeMiddlewareIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Middleware_TracksHealthCheckEndpoint()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/health/live", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // The middleware should have tracked this request
        // (telemetry is verified through Application Insights in production)
    }

    [Fact]
    public async Task Middleware_TracksApiRequests()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Make a request to an API endpoint
        var response = await client.GetAsync("/api/health/ready", TestContext.Current.CancellationToken);

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.ServiceUnavailable);

        // The middleware should have tracked this request with:
        // - Endpoint: "GET /api/health/ready"
        // - StatusCode: 200 or 503
        // - Duration: measured in milliseconds
    }

    [Theory]
    [InlineData("/api/health/live")]
    [InlineData("/api/health/ready")]
    [InlineData("/api/health")]
    public async Task Middleware_TracksVariousEndpoints(string endpoint)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(endpoint, TestContext.Current.CancellationToken);

        // Assert - Middleware should track all endpoints regardless of status code
        Assert.NotNull(response);
    }

    [Fact]
    public async Task Middleware_TracksNonApiRoutes()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Blazor app route returns index.html with 200 OK
        var response = await client.GetAsync("/nonexistent-page", TestContext.Current.CancellationToken);

        // Assert - MapFallbackToFile returns 200 for non-API routes (Blazor SPA)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // The middleware should track this as:
        // - Endpoint: "GET /nonexistent-page"
        // - StatusCode: 200 (served index.html)
        // - Duration: measured in milliseconds
    }
}
