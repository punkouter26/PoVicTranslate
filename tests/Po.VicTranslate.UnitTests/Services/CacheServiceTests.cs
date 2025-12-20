using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Po.VicTranslate.Api.Services.Caching;
using Xunit;

namespace Po.VicTranslate.UnitTests.Services;

public class CacheServiceTests : IDisposable
{
    private readonly Mock<ILogger<CacheService>> _mockLogger;
    private readonly IMemoryCache _memoryCache;
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
        var expectedValue = "test-value";

        // First call to populate cache
        await _cacheService.GetOrCreateAsync(
            key,
            async () => await Task.FromResult(expectedValue));

        var factoryExecuted = false;

        // Act - Second call should hit cache
        var result = await _cacheService.GetOrCreateAsync(
            key,
            async () =>
            {
                factoryExecuted = true;
                return await Task.FromResult("different-value");
            });

        // Assert
        factoryExecuted.Should().BeFalse();
        result.Should().Be(expectedValue);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithAbsoluteExpiration_ShouldExpireAfterTime()
    {
        // Arrange
        var key = "test-key";
        var firstValue = "first-value";
        var secondValue = "second-value";

        // Act - First call with very short expiration (50ms)
        await _cacheService.GetOrCreateAsync(
            key,
            async () => await Task.FromResult(firstValue),
            absoluteExpiration: TimeSpan.FromMilliseconds(50));

        // Wait for expiration
        await Task.Delay(100, TestContext.Current.CancellationToken);

        // Second call should execute factory again
        var result = await _cacheService.GetOrCreateAsync(
            key,
            async () => await Task.FromResult(secondValue));

        // Assert
        result.Should().Be(secondValue);
    }

    [Fact]
    public void GetStatistics_WithNoActivity_ShouldReturnZeros()
    {
        // Act
        var stats = _cacheService.GetStatistics();

        // Assert
        stats.TotalHits.Should().Be(0);
        stats.TotalMisses.Should().Be(0);
        stats.TotalEvictions.Should().Be(0);
        stats.HitRate.Should().Be(0);
    }

    [Fact]
    public async Task GetStatistics_AfterCacheMiss_ShouldIncrementMisses()
    {
        // Arrange
        var key = "test-key";

        // Act
        await _cacheService.GetOrCreateAsync(
            key,
            async () => await Task.FromResult("value"));

        var stats = _cacheService.GetStatistics();

        // Assert
        stats.TotalMisses.Should().Be(1);
        stats.TotalHits.Should().Be(0);
    }

    [Fact]
    public async Task GetStatistics_AfterCacheHit_ShouldIncrementHits()
    {
        // Arrange
        var key = "test-key";

        // First call - miss
        await _cacheService.GetOrCreateAsync(
            key,
            async () => await Task.FromResult("value"));

        // Act - Second call - hit
        await _cacheService.GetOrCreateAsync(
            key,
            async () => await Task.FromResult("value"));

        var stats = _cacheService.GetStatistics();

        // Assert
        stats.TotalHits.Should().Be(1);
        stats.TotalMisses.Should().Be(1);
    }

    [Fact]
    public async Task GetStatistics_ShouldCalculateHitRate()
    {
        // Arrange
        var key = "test-key";

        // 1 miss
        await _cacheService.GetOrCreateAsync(
            key,
            async () => await Task.FromResult("value"));

        // 3 hits
        await _cacheService.GetOrCreateAsync(key, async () => await Task.FromResult("value"));
        await _cacheService.GetOrCreateAsync(key, async () => await Task.FromResult("value"));
        await _cacheService.GetOrCreateAsync(key, async () => await Task.FromResult("value"));

        // Act
        var stats = _cacheService.GetStatistics();

        // Assert
        stats.TotalHits.Should().Be(3);
        stats.TotalMisses.Should().Be(1);
        stats.HitRate.Should().Be(75.0); // 3/4 = 75%
    }

    [Fact]
    public void Remove_ShouldRemoveEntryFromCache()
    {
        // Arrange
        var key = "test-key";
        _memoryCache.Set(key, "value");

        // Act
        _cacheService.Remove(key);

        // Assert
        _memoryCache.TryGetValue(key, out var _).Should().BeFalse();
    }

    [Fact]
    public async Task RemoveByPrefix_ShouldRemoveMatchingEntries()
    {
        // Arrange
        await _cacheService.GetOrCreateAsync("prefix:key1", async () => await Task.FromResult("value1"));
        await _cacheService.GetOrCreateAsync("prefix:key2", async () => await Task.FromResult("value2"));
        await _cacheService.GetOrCreateAsync("other:key3", async () => await Task.FromResult("value3"));

        // Act
        _cacheService.RemoveByPrefix("prefix:");

        // Assert
        _memoryCache.TryGetValue("prefix:key1", out var _).Should().BeFalse();
        _memoryCache.TryGetValue("prefix:key2", out var _).Should().BeFalse();
        _memoryCache.TryGetValue("other:key3", out var _).Should().BeTrue();
    }

    [Fact]
    public async Task RemoveByPrefix_WithNoMatches_ShouldNotThrow()
    {
        // Arrange
        await _cacheService.GetOrCreateAsync("key1", async () => await Task.FromResult("value1"));

        // Act
        var act = () => _cacheService.RemoveByPrefix("nonexistent:");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task Clear_ShouldRemoveAllEntries()
    {
        // Arrange
        await _cacheService.GetOrCreateAsync("key1", async () => await Task.FromResult("value1"));
        await _cacheService.GetOrCreateAsync("key2", async () => await Task.FromResult("value2"));
        await _cacheService.GetOrCreateAsync("key3", async () => await Task.FromResult("value3"));

        // Act
        _cacheService.Clear();

        // Assert
        _memoryCache.TryGetValue("key1", out var _).Should().BeFalse();
        _memoryCache.TryGetValue("key2", out var _).Should().BeFalse();
        _memoryCache.TryGetValue("key3", out var _).Should().BeFalse();
    }

    [Fact]
    public async Task GetOrCreateAsync_WithComplexType_ShouldCache()
    {
        // Arrange
        var key = "complex-key";
        var complexObject = new TestComplexType
        {
            Id = 1,
            Name = "Test",
            Values = new List<string> { "a", "b", "c" }
        };

        // Act
        var result = await _cacheService.GetOrCreateAsync(
            key,
            async () => await Task.FromResult(complexObject));

        var cachedResult = await _cacheService.GetOrCreateAsync(
            key,
            async () => await Task.FromResult(new TestComplexType { Id = 999 }));

        // Assert
        cachedResult.Should().BeSameAs(complexObject);
        cachedResult.Id.Should().Be(1);
    }

    private class TestComplexType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Values { get; set; } = new();
    }
}
