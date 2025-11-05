using Xunit;
using FluentAssertions;
using Po.VicTranslate.Api.Services.BrowserLog;

namespace Po.VicTranslate.UnitTests.Services.BrowserLog;

public class BrowserLogStrategyFactoryTests
{
    [Fact]
    public void GetStrategy_WithEventLogType_ReturnsEventLogStrategy()
    {
        // Arrange
        var strategies = new IBrowserLogStrategy[]
        {
            new EventLogStrategy(),
            new InstabilityLogStrategy(),
            new FailureLogStrategy(),
            new UnknownLogStrategy()
        };
        var factory = new BrowserLogStrategyFactory(strategies);

        // Act
        var strategy = factory.GetStrategy("event");

        // Assert
        strategy.Should().BeOfType<EventLogStrategy>();
    }

    [Fact]
    public void GetStrategy_WithInstabilityLogType_ReturnsInstabilityLogStrategy()
    {
        // Arrange
        var strategies = new IBrowserLogStrategy[]
        {
            new EventLogStrategy(),
            new InstabilityLogStrategy(),
            new FailureLogStrategy(),
            new UnknownLogStrategy()
        };
        var factory = new BrowserLogStrategyFactory(strategies);

        // Act
        var strategy = factory.GetStrategy("instability");

        // Assert
        strategy.Should().BeOfType<InstabilityLogStrategy>();
    }

    [Fact]
    public void GetStrategy_WithFailureLogType_ReturnsFailureLogStrategy()
    {
        // Arrange
        var strategies = new IBrowserLogStrategy[]
        {
            new EventLogStrategy(),
            new InstabilityLogStrategy(),
            new FailureLogStrategy(),
            new UnknownLogStrategy()
        };
        var factory = new BrowserLogStrategyFactory(strategies);

        // Act
        var strategy = factory.GetStrategy("failure");

        // Assert
        strategy.Should().BeOfType<FailureLogStrategy>();
    }

    [Fact]
    public void GetStrategy_WithUnknownLogType_ReturnsUnknownLogStrategy()
    {
        // Arrange
        var strategies = new IBrowserLogStrategy[]
        {
            new EventLogStrategy(),
            new InstabilityLogStrategy(),
            new FailureLogStrategy(),
            new UnknownLogStrategy()
        };
        var factory = new BrowserLogStrategyFactory(strategies);

        // Act
        var strategy = factory.GetStrategy("unknown-type");

        // Assert
        strategy.Should().BeOfType<UnknownLogStrategy>();
    }

    [Fact]
    public void GetStrategy_WithNullLogType_ReturnsUnknownLogStrategy()
    {
        // Arrange
        var strategies = new IBrowserLogStrategy[]
        {
            new EventLogStrategy(),
            new InstabilityLogStrategy(),
            new FailureLogStrategy(),
            new UnknownLogStrategy()
        };
        var factory = new BrowserLogStrategyFactory(strategies);

        // Act
        var strategy = factory.GetStrategy(null);

        // Assert
        strategy.Should().BeOfType<UnknownLogStrategy>();
    }

    [Fact]
    public void Constructor_WithNullStrategies_ThrowsArgumentNullException()
    {
        // Act
        var act = () =>
        {
            var factory = new BrowserLogStrategyFactory(null!);
            return factory;
        };

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("strategies");
    }
}
