using Xunit;
using FluentAssertions;
using Po.VicTranslate.Api.Services.ClientLog;
using Po.VicTranslate.Api.Controllers;

namespace Po.VicTranslate.UnitTests.Services.ClientLog;

public class ClientLogHandlerFactoryTests
{
    [Fact]
    public void GetHandler_WithErrorLogLevel_ReturnsErrorLogHandler()
    {
        // Arrange
        var handlers = new IClientLogHandler[]
        {
            new ErrorLogHandler(),
            new WarningLogHandler(),
            new InfoLogHandler()
        };
        var factory = new ClientLogHandlerFactory(handlers);
        var logEntry = new ClientLogEntry { Level = "error" };

        // Act
        var handler = factory.GetHandler(logEntry);

        // Assert
        handler.Should().BeOfType<ErrorLogHandler>();
    }

    [Fact]
    public void GetHandler_WithWarningLogLevel_ReturnsWarningLogHandler()
    {
        // Arrange
        var handlers = new IClientLogHandler[]
        {
            new ErrorLogHandler(),
            new WarningLogHandler(),
            new InfoLogHandler()
        };
        var factory = new ClientLogHandlerFactory(handlers);
        var logEntry = new ClientLogEntry { Level = "warning" };

        // Act
        var handler = factory.GetHandler(logEntry);

        // Assert
        handler.Should().BeOfType<WarningLogHandler>();
    }

    [Fact]
    public void GetHandler_WithInfoLogLevel_ReturnsInfoLogHandler()
    {
        // Arrange
        var handlers = new IClientLogHandler[]
        {
            new ErrorLogHandler(),
            new WarningLogHandler(),
            new InfoLogHandler()
        };
        var factory = new ClientLogHandlerFactory(handlers);
        var logEntry = new ClientLogEntry { Level = "info" };

        // Act
        var handler = factory.GetHandler(logEntry);

        // Assert
        handler.Should().BeOfType<InfoLogHandler>();
    }

    [Fact]
    public void GetHandler_WithUnknownLogLevel_ThrowsInvalidOperationException()
    {
        // Arrange
        var handlers = new IClientLogHandler[]
        {
            new ErrorLogHandler(),
            new WarningLogHandler(),
            new InfoLogHandler()
        };
        var factory = new ClientLogHandlerFactory(handlers);
        var logEntry = new ClientLogEntry { Level = "unknown" };

        // Act
        Action act = () => factory.GetHandler(logEntry);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No handler found for log level: unknown*");
    }

    [Fact]
    public void Constructor_WithNullHandlers_ThrowsArgumentNullException()
    {
        // Act
        var act = () =>
        {
            var factory = new ClientLogHandlerFactory(null!);
            return factory;
        };

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("handlers");
    }
}
