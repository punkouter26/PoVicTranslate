using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using PoVicTranslate.Web.Services.Caching;
using Xunit;

namespace Po.VicTranslate.UnitTests.Services;

public class CacheServiceTests : IDisposable
{
    private readonly Mock<ILogger<CacheService>> _mockLogger;
    private readonly MemoryCache _memoryCache;
    private readonly CacheService _cacheService;

    public CacheServiceTests()
    {
        _mockLogger = new Mock<ILogger<CacheService>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _cacheService = new CacheService(_memoryCache, _mockLogger.Object);
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithCacheMiss_ShouldExecuteFactory()
    {
        // Arrange
        var key = "test-key";
        var expectedValue = "test-value";
        var factoryExecuted = false;

        // Act
        var result = await _cacheService.GetOrCreateAsync(
            key,
            async () =>
            {
                factoryExecuted = true;
                return await Task.FromResult(expectedValue);
            });

        // Assert
        factoryExecuted.Should().BeTrue();
        result.Should().Be(expectedValue);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithCacheHit_ShouldNotExecuteFactory()
    {
        // Arrange
        var key = "test-key";
        var expectedValue = "cached-value";
        var factoryCallCount = 0;

        // First call to populate cache
        await _cacheService.GetOrCreateAsync(
            key,
            async () =>
            {
                factoryCallCount++;
                return await Task.FromResult(expectedValue);
            });

        // Act - Second call should hit cache
        var result = await _cacheService.GetOrCreateAsync(
            key,
            async () =>
            {
                factoryCallCount++;
                return await Task.FromResult("new-value");
            });

        // Assert
        factoryCallCount.Should().Be(1);
        result.Should().Be(expectedValue);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithCustomExpiration_ShouldExpireAfterTime()
    {
        // Arrange
        var key = "expiring-key";
        var initialValue = "initial";
        var updatedValue = "updated";
        var shortExpiration = TimeSpan.FromMilliseconds(100);

        // Populate cache with short expiration
        await _cacheService.GetOrCreateAsync(
            key,
            () => Task.FromResult(initialValue),
            shortExpiration);

        // Wait for expiration
        await Task.Delay(150, TestContext.Current.CancellationToken);

        // Act - Should get new value
        var result = await _cacheService.GetOrCreateAsync(
            key,
            () => Task.FromResult(updatedValue),
            shortExpiration);

        // Assert
        result.Should().Be(updatedValue);
    }

    [Fact]
    public async Task Remove_ShouldRemoveEntryFromCache()
    {
        // Arrange
        var key = "remove-test-key";
        var initialValue = "initial";
        var newValue = "new";
        var factoryCallCount = 0;

        await _cacheService.GetOrCreateAsync(
            key,
            () =>
            {
                factoryCallCount++;
                return Task.FromResult(initialValue);
            });

        // Act
        _cacheService.Remove(key);

        var result = await _cacheService.GetOrCreateAsync(
            key,
            () =>
            {
                factoryCallCount++;
                return Task.FromResult(newValue);
            });

        // Assert
        factoryCallCount.Should().Be(2);
        result.Should().Be(newValue);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithComplexType_ShouldCache()
    {
        // Arrange
        var key = "complex-key";
        var expectedValue = new TestDto { Id = 1, Name = "Test" };
        var factoryCallCount = 0;

        // Act
        var firstResult = await _cacheService.GetOrCreateAsync(
            key,
            () =>
            {
                factoryCallCount++;
                return Task.FromResult(expectedValue);
            });

        var secondResult = await _cacheService.GetOrCreateAsync(
            key,
            () =>
            {
                factoryCallCount++;
                return Task.FromResult(new TestDto { Id = 2, Name = "Different" });
            });

        // Assert
        factoryCallCount.Should().Be(1);
        firstResult.Should().BeEquivalentTo(expectedValue);
        secondResult.Should().BeEquivalentTo(expectedValue);
    }

    [Fact]
    public void Remove_NonExistentKey_ShouldNotThrow()
    {
        // Arrange
        var key = "non-existent-key";

        // Act
        var act = () => _cacheService.Remove(key);

        // Assert
        act.Should().NotThrow();
    }

    private sealed class TestDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}
