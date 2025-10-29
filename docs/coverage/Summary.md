# Summary

|||
|:---|:---|
| Generated on: | 10/29/2025 - 3:13:48 PM |
| Coverage date: | 10/29/2025 - 3:13:10 PM - 10/29/2025 - 3:13:16 PM |
| Parser: | MultiReport (2x Cobertura) |
| Assemblies: | 2 |
| Classes: | 53 |
| Files: | 40 |
| **Line coverage:** | 33.7% (773 of 2287) |
| Covered lines: | 773 |
| Uncovered lines: | 1514 |
| Coverable lines: | 2287 |
| Total lines: | 4094 |
| **Branch coverage:** | 22.7% (105 of 462) |
| Covered branches: | 105 |
| Total branches: | 462 |
| **Method coverage:** | [Feature is only available for sponsors](https://reportgenerator.io/pro) |

# Risk Hotspots

| **Assembly** | **Class** | **Method** | **Crap Score** | **Cyclomatic complexity** |
|:---|:---|:---|---:|---:|
| Po.VicTranslate.Api | Po.VicTranslate.Api.Controllers.DebugController | ReceiveBrowserLog() | 1640 | 40 || Po.VicTranslate.Api | Po.VicTranslate.Api.Controllers.ClientLogController | PostClientLog(...) | 600 | 24 || Po.VicTranslate.Api | Po.VicTranslate.Api.Services.LyricsManagementService | GenerateTags(...) | 272 | 16 || Po.VicTranslate.Api | Po.VicTranslate.Api.Services.ConfigurationValidator | ValidateAzureOpenAI() | 210 | 14 || Po.VicTranslate.Api | Po.VicTranslate.Api.Services.ConfigurationValidator | ValidateAzureSpeechAsync() | 210 | 14 || Po.VicTranslate.Api | Po.VicTranslate.Api.Services.LyricsManagementService | ParseSongMetadata(...) | 210 | 14 || Po.VicTranslate.Client | Po.VicTranslate.Client.Components.Pages.Diag | BuildRenderTree(...) | 210 | 14 || Po.VicTranslate.Client | VictorianTranslator.Components.Pages.Translation | BuildRenderTree(...) | 210 | 14 || Po.VicTranslate.Client | Po.VicTranslate.Client.ViewModels.TranslationViewModel | SetProperty(...) | 110 | 10 || Po.VicTranslate.Api | Po.VicTranslate.Api.Controllers.DebugController | LogTestFailure() | 72 | 8 || Po.VicTranslate.Api | Po.VicTranslate.Api.Services.LyricsManagementService | RegenerateLyricsCollectionAsync() | 72 | 8 || Po.VicTranslate.Client | Po.VicTranslate.Client.Extensions.StringExtensions | ToTitleCase(...) | 72 | 8 || Po.VicTranslate.Api | Po.VicTranslate.Api.HealthChecks.AzureOpenAIHealthCheck | CheckHealthAsync(...) | 60 | 14 || Po.VicTranslate.Api | Po.VicTranslate.Api.Services.TranslationService | TranslateToVictorianEnglishAsync() | 47 | 20 || Po.VicTranslate.Api | Po.VicTranslate.Api.Services.DebugLogService | GetLogsInTimeRangeAsync() | 42 | 6 || Po.VicTranslate.Api | Po.VicTranslate.Api.Services.DebugLogService | GetRecentLogsAsync() | 42 | 6 || Po.VicTranslate.Client | Po.VicTranslate.Client.Services.TranslationOrchestrator | TranslateTextAsync() | 42 | 6 || Po.VicTranslate.Api | Po.VicTranslate.Api.HealthChecks.AzureSpeechHealthCheck | CheckHealthAsync() | 33 | 14 || Po.VicTranslate.Api | Po.VicTranslate.Api.Middleware.ProblemDetailsExceptionHandler | CreateProblemDetails(...) | 17 | 16 |
# Coverage

| **Name** | **Covered** | **Uncovered** | **Coverable** | **Total** | **Line coverage** | **Covered** | **Total** | **Branch coverage** |
|:---|---:|---:|---:|---:|---:|---:|---:|---:|
| **Po.VicTranslate.Api** | **773** | **1072** | **1845** | **5211** | **41.8%** | **105** | **376** | **27.9%** |
| Po.VicTranslate.Api.Configuration.ApiSettings | 5 | 0 | 5 | 12 | 100% | 0 | 0 |  |
| Po.VicTranslate.Api.Controllers.BrowserLogPayload | 0 | 12 | 12 | 294 | 0% | 0 | 0 |  |
| Po.VicTranslate.Api.Controllers.BrowserLogRequest | 0 | 2 | 2 | 294 | 0% | 0 | 0 |  |
| Po.VicTranslate.Api.Controllers.ClientLogController | 0 | 41 | 41 | 91 | 0% | 0 | 24 | 0% |
| Po.VicTranslate.Api.Controllers.ClientLogEntry | 0 | 7 | 7 | 91 | 0% | 0 | 0 |  |
| Po.VicTranslate.Api.Controllers.DebugController | 0 | 146 | 146 | 294 | 0% | 0 | 60 | 0% |
| Po.VicTranslate.Api.Controllers.HealthController | 30 | 33 | 63 | 121 | 47.6% | 2 | 16 | 12.5% |
| Po.VicTranslate.Api.Controllers.LyricsController | 28 | 1 | 29 | 63 | 96.5% | 5 | 6 | 83.3% |
| Po.VicTranslate.Api.Controllers.LyricsManagementController | 0 | 96 | 96 | 158 | 0% | 0 | 6 | 0% |
| Po.VicTranslate.Api.Controllers.SpeechController | 0 | 32 | 32 | 58 | 0% | 0 | 4 | 0% |
| Po.VicTranslate.Api.Controllers.TestEventRequest | 0 | 3 | 3 | 294 | 0% | 0 | 0 |  |
| Po.VicTranslate.Api.Controllers.TestFailureRequest | 0 | 5 | 5 | 294 | 0% | 0 | 0 |  |
| Po.VicTranslate.Api.Controllers.TestInstabilityRequest | 0 | 3 | 3 | 294 | 0% | 0 | 0 |  |
| Po.VicTranslate.Api.Controllers.TranslationController | 12 | 0 | 12 | 31 | 100% | 2 | 2 | 100% |
| Po.VicTranslate.Api.HealthChecks.AzureOpenAIHealthCheck | 19 | 21 | 40 | 68 | 47.5% | 4 | 14 | 28.5% |
| Po.VicTranslate.Api.HealthChecks.AzureSpeechHealthCheck | 26 | 17 | 43 | 74 | 60.4% | 6 | 14 | 42.8% |
| Po.VicTranslate.Api.HealthChecks.InternetConnectivityHealthCheck | 14 | 12 | 26 | 50 | 53.8% | 1 | 2 | 50% |
| Po.VicTranslate.Api.Middleware.DebugLoggingMiddleware | 118 | 15 | 133 | 178 | 88.7% | 11 | 14 | 78.5% |
| Po.VicTranslate.Api.Middleware.ProblemDetailsExceptionHandler | 55 | 8 | 63 | 101 | 87.3% | 6 | 16 | 37.5% |
| Po.VicTranslate.Api.Models.AppStateSnapshot | 0 | 7 | 7 | 97 | 0% | 0 | 0 |  |
| Po.VicTranslate.Api.Models.DebugLogEntry | 18 | 10 | 28 | 97 | 64.2% | 2 | 4 | 50% |
| Po.VicTranslate.Api.Models.DebugSummaryReport | 0 | 7 | 7 | 97 | 0% | 0 | 0 |  |
| Po.VicTranslate.Api.Models.DiagnosticResult | 3 | 1 | 4 | 27 | 75% | 0 | 0 |  |
| Po.VicTranslate.Api.Models.EventSummary | 0 | 7 | 7 | 97 | 0% | 0 | 0 |  |
| Po.VicTranslate.Api.Models.LyricsCollection | 6 | 0 | 6 | 70 | 100% | 0 | 0 |  |
| Po.VicTranslate.Api.Models.LyricsSearchResult | 3 | 0 | 3 | 70 | 100% | 0 | 0 |  |
| Po.VicTranslate.Api.Models.PerformanceMetric | 0 | 6 | 6 | 97 | 0% | 0 | 0 |  |
| Po.VicTranslate.Api.Models.Song | 12 | 0 | 12 | 70 | 100% | 0 | 0 |  |
| Po.VicTranslate.Api.Models.SystemResourceUsage | 0 | 6 | 6 | 97 | 0% | 0 | 0 |  |
| Po.VicTranslate.Api.Models.TranslationRequest | 1 | 0 | 1 | 6 | 100% | 0 | 0 |  |
| Po.VicTranslate.Api.Models.TranslationResponse | 1 | 0 | 1 | 6 | 100% | 0 | 0 |  |
| Po.VicTranslate.Api.Services.AudioSynthesisService | 26 | 16 | 42 | 73 | 61.9% | 6 | 10 | 60% |
| Po.VicTranslate.Api.Services.ConfigurationValidator | 0 | 133 | 133 | 178 | 0% | 0 | 30 | 0% |
| Po.VicTranslate.Api.Services.DebugLogService | 103 | 156 | 259 | 343 | 39.7% | 15 | 40 | 37.5% |
| Po.VicTranslate.Api.Services.DiagnosticService | 0 | 14 | 14 | 34 | 0% | 0 | 0 |  |
| Po.VicTranslate.Api.Services.LyricsManagementService | 90 | 162 | 252 | 369 | 35.7% | 18 | 62 | 29% |
| Po.VicTranslate.Api.Services.LyricsService | 27 | 22 | 49 | 96 | 55.1% | 5 | 12 | 41.6% |
| Po.VicTranslate.Api.Services.TranslationService | 73 | 33 | 106 | 235 | 68.8% | 15 | 26 | 57.6% |
| Program | 103 | 38 | 141 | 192 | 73% | 7 | 14 | 50% |
| **Po.VicTranslate.Client** | **0** | **442** | **442** | **1069** | **0%** | **0** | **86** | **0%** |
| Po.VicTranslate.Client.Components.Pages.DebugErrors | 0 | 86 | 86 | 149 | 0% | 0 | 2 | 0% |
| Po.VicTranslate.Client.Components.Pages.Diag | 0 | 59 | 59 | 184 | 0% | 0 | 14 | 0% |
| Po.VicTranslate.Client.Components.Pages.Error | 0 | 7 | 7 | 33 | 0% | 0 | 4 | 0% |
| Po.VicTranslate.Client.Extensions.StringExtensions | 0 | 14 | 14 | 22 | 0% | 0 | 8 | 0% |
| Po.VicTranslate.Client.Models.TranslationRequest | 0 | 1 | 1 | 6 | 0% | 0 | 0 |  |
| Po.VicTranslate.Client.Models.TranslationResponse | 0 | 1 | 1 | 6 | 0% | 0 | 0 |  |
| Po.VicTranslate.Client.Services.ClientLyricsService | 0 | 10 | 10 | 26 | 0% | 0 | 2 | 0% |
| Po.VicTranslate.Client.Services.ClientSpeechService | 0 | 12 | 12 | 28 | 0% | 0 | 2 | 0% |
| Po.VicTranslate.Client.Services.ClientTranslationService | 0 | 11 | 11 | 25 | 0% | 0 | 2 | 0% |
| Po.VicTranslate.Client.Services.TranslationOrchestrator | 0 | 109 | 109 | 184 | 0% | 0 | 10 | 0% |
| Po.VicTranslate.Client.ViewModels.TranslationViewModel | 0 | 57 | 57 | 121 | 0% | 0 | 20 | 0% |
| Program | 0 | 20 | 20 | 29 | 0% | 0 | 2 | 0% |
| VictorianTranslator.Client.Components.Layout.MainLayout | 0 | 1 | 1 | 40 | 0% | 0 | 0 |  |
| VictorianTranslator.Components.Pages.Translation | 0 | 54 | 54 | 216 | 0% | 0 | 20 | 0% |

