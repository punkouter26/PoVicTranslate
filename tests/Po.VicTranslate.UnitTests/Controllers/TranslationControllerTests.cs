// TODO: These tests were for TranslationController which has been converted to Minimal API endpoints (TranslationEndpoints).
// The endpoint methods are now static extension methods and need different testing approaches.
// Consider testing the ITranslationService directly or using integration tests for the endpoints.

/*
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PoVicTranslate.Web.Models;
using PoVicTranslate.Web.Endpoints; // Note: Endpoints are now static extension classes
using PoVicTranslate.Web.Services;
using PoVicTranslate.Web.Services.Validation;
using Xunit;

namespace Po.VicTranslate.UnitTests.Controllers;

public class TranslationControllerTests
{
    private readonly Mock<ITranslationService> _mockTranslationService;
    private readonly Mock<IAudioSynthesisService> _mockAudioSynthesisService;
    private readonly Mock<ICustomTelemetryService> _mockTelemetryService;
    private readonly Mock<IInputValidator> _mockInputValidator;
    // TranslationController no longer exists - converted to TranslationEndpoints (Minimal API)

    public TranslationControllerTests()
    {
        _mockTranslationService = new Mock<ITranslationService>();
        _mockAudioSynthesisService = new Mock<IAudioSynthesisService>();
        _mockTelemetryService = new Mock<ICustomTelemetryService>();
        _mockInputValidator = new Mock<IInputValidator>();
    }

    [Fact]
    public async Task Translate_WithValidRequest_ShouldReturnOkResult()
    {
        // TODO: Test via integration tests or test ITranslationService directly
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Translate_WithEmptyText_ShouldReturnBadRequest(string? text)
    {
        // TODO: Test via integration tests or test ITranslationService directly
    }

    [Fact]
    public async Task Translate_ShouldCallTranslationService()
    {
        // TODO: Test via integration tests or test ITranslationService directly
    }

    [Fact]
    public async Task Translate_WhenServiceThrowsException_ShouldPropagate()
    {
        // TODO: Test via integration tests or test ITranslationService directly
    }
}
*/
