# PoVicTranslate

**Victorian English Translation & Lyrics Management System**

A full-stack web application that translates modern English to Victorian-era English using Azure OpenAI, manages song lyrics with Cockney rhyming slang support, and provides text-to-speech synthesis. Built with ASP.NET Core 9.0 and Blazor WebAssembly.

---

## 🎯 Project Overview

PoVicTranslate is a modern web application that bridges contemporary and Victorian English through AI-powered translation. It combines natural language processing, speech synthesis, and a comprehensive lyrics management system to provide an interactive experience with historical English dialects.

### Key Features

- **AI-Powered Translation**: Converts modern English to Victorian-era English using Azure OpenAI GPT-4
- **Lyrics Management**: Full CRUD operations for song lyrics with Cockney rhyming slang support
- **Audio Synthesis**: Text-to-speech conversion using Azure Cognitive Services
- **Real-time Telemetry**: Application Insights integration with custom metrics
- **Health Monitoring**: Comprehensive health checks for all external dependencies
- **Interactive UI**: Modern Blazor WebAssembly client with responsive design

---

## 🏗️ Architecture

### Technology Stack

- **Backend**: ASP.NET Core 9.0 Web API
- **Frontend**: Blazor WebAssembly (.NET 9.0)
- **Cloud Services**: 
  - Azure OpenAI (GPT-4)
  - Azure Cognitive Services (Speech)
  - Azure Application Insights
  - Azure App Service
- **Logging**: Serilog with Application Insights sink
- **Testing**: xUnit, FluentAssertions, Playwright
- **CI/CD**: GitHub Actions with federated credentials

### Project Structure

```
PoVicTranslate/
├── src/
│   ├── Po.VicTranslate.Api/          # ASP.NET Core Web API
│   └── Po.VicTranslate.Client/       # Blazor WebAssembly Client
├── tests/
│   ├── Po.VicTranslate.UnitTests/    # Unit tests (xUnit)
│   ├── Po.VicTranslate.IntegrationTests/  # Integration tests
│   └── Po.VicTranslate.E2ETests/     # End-to-end tests (Playwright)
├── infra/                            # Azure Bicep infrastructure
├── docs/                             # Documentation and diagrams
└── .github/workflows/                # CI/CD pipeline
```

---

## 🚀 Quick Start

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
- [Node.js 18+](https://nodejs.org/) (for E2E tests)
- Azure Subscription with:
  - Azure OpenAI Service access
  - Azure Cognitive Services (Speech)

### Local Development Setup

1. **Clone the repository**
   ```powershell
   git clone https://github.com/punkouter26/PoVicTranslate.git
   cd PoVicTranslate
   ```

2. **Configure User Secrets**
   ```powershell
   cd src/Po.VicTranslate.Api
   dotnet user-secrets set "ApiSettings:AzureOpenAIEndpoint" "your-endpoint"
   dotnet user-secrets set "ApiSettings:AzureOpenAIApiKey" "your-key"
   dotnet user-secrets set "ApiSettings:AzureOpenAIDeploymentName" "gpt-4"
   dotnet user-secrets set "ApiSettings:AzureSpeechRegion" "eastus2"
   dotnet user-secrets set "ApiSettings:AzureSpeechSubscriptionKey" "your-key"
   ```

3. **Run the Application**
   ```powershell
   # From repository root
   dotnet run --project src/Po.VicTranslate.Api/Po.VicTranslate.Api.csproj
   ```

4. **Access the Application**
   - API: https://localhost:7070
   - Swagger UI: https://localhost:7070/swagger
   - Blazor Client: https://localhost:7070 (served by API)

### Running Tests

```powershell
# Run all unit and integration tests
dotnet test

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run E2E tests (Playwright)
cd tests/Po.VicTranslate.E2ETests
npm install
npx playwright test
```

---

## 📦 Projects

### Po.VicTranslate.Api

**ASP.NET Core 9.0 Web API** - The backend service providing all business logic and external service integration.

**Key Components:**
- **Controllers**: RESTful API endpoints for translation, lyrics, and audio
- **Services**: Business logic layer for translation, lyrics management, and audio synthesis
- **Health Checks**: Azure OpenAI, Speech Service, and Internet connectivity monitoring
- **Middleware**: Exception handling, debug logging, and telemetry enrichment
- **Configuration**: Azure service settings, Application Insights, and Serilog

**Key Technologies:**
- Serilog (structured logging)
- Application Insights (telemetry)
- Azure SDK for .NET
- ASP.NET Core Health Checks

**API Endpoints:**
- `GET /api/health` - Full health check with dependency status
- `GET /api/health/live` - Liveness probe
- `GET /api/health/ready` - Readiness probe
- `POST /api/translation/translate` - Translate text to Victorian English
- `GET /api/lyrics` - Get all lyrics
- `POST /api/lyrics` - Add new lyrics
- `PUT /api/lyrics/{id}` - Update lyrics
- `DELETE /api/lyrics/{id}` - Delete lyrics
- `POST /api/audio/synthesize` - Generate speech audio

### Po.VicTranslate.Client

**Blazor WebAssembly (.NET 9.0)** - Interactive single-page application providing the user interface.

**Key Components:**
- **Pages**: Translation, Lyrics Management, Audio Synthesis
- **Components**: Reusable UI components (TranslationForm, LyricsGrid, AudioPlayer)
- **Services**: HTTP client services for API communication
- **ViewModels**: State management and presentation logic
- **Extensions**: Service registration and dependency injection configuration

**Key Features:**
- Responsive Material Design-inspired UI
- Real-time translation feedback
- Interactive lyrics management grid
- Audio playback with browser controls
- Client-side validation

### Po.VicTranslate.UnitTests

**xUnit Test Project** - Comprehensive unit tests for all business logic.

**Coverage:**
- Service layer tests (Translation, Lyrics, Audio)
- Controller tests with mocked dependencies
- Health check tests
- Configuration validation tests
- Middleware tests

**Test Technologies:**
- xUnit 3.1.0
- Moq 4.20.72
- FluentAssertions 8.8.0

**Current Coverage:** 33.7% line coverage, 22.7% branch coverage (see `docs/CODE_COVERAGE.md`)

### Po.VicTranslate.IntegrationTests

**Integration Test Project** - Tests for API endpoints and system integration.

**Test Coverage:**
- Health endpoint integration
- Translation endpoint with real Azure services
- Lyrics CRUD operations
- Error handling and validation

### Po.VicTranslate.E2ETests

**Playwright Test Suite** - End-to-end browser automation tests.

**Test Scenarios:**
- Full translation workflow
- Lyrics management user journeys
- Audio synthesis and playback
- Error handling and edge cases

---

## 🔧 Configuration

### Application Settings

Configuration follows the ASP.NET Core configuration hierarchy:

1. `appsettings.json` (base configuration)
2. `appsettings.Development.json` (development overrides)
3. User Secrets (local development)
4. Environment Variables (production)

### Required Settings

```json
{
  "ApiSettings": {
    "AzureOpenAIEndpoint": "https://your-openai.openai.azure.com/",
    "AzureOpenAIApiKey": "your-key",
    "AzureOpenAIDeploymentName": "gpt-4",
    "AzureSpeechRegion": "eastus2",
    "AzureSpeechSubscriptionKey": "your-key"
  },
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=...;IngestionEndpoint=..."
  }
}
```

---

## 📊 Monitoring & Observability

### Structured Logging (Serilog)

All application events are logged with structured properties for easy querying:

- **Console Sink**: Development debugging
- **Debug Sink**: Visual Studio output
- **File Sink**: Local file logging (development only)
- **Application Insights Sink**: Production telemetry

### Custom Telemetry Events

Six custom event types tracked in Application Insights:

1. **TranslationRequest**: Translation attempts, success/failure, timing
2. **LyricsAccess**: Lyrics CRUD operations
3. **AudioSynthesis**: TTS generation metrics
4. **DataUsage**: API call volumes and patterns
5. **PerformanceMetric**: Response times and throughput
6. **UserActivity**: User engagement and sessions

### KQL Queries

Pre-built Kusto queries available in `docs/KQL/`:
- User activity analysis (DAU, sessions, retention)
- Performance monitoring (P50/P95/P99, slow requests)
- Error rate tracking (availability SLA, spike detection)
- Custom telemetry analytics

---

## 🚢 Deployment

### Azure Resources Required

- **App Service**: Windows, .NET 9.0 runtime (F1 or higher)
- **Application Insights**: Telemetry and monitoring
- **Log Analytics Workspace**: Centralized logging
- **Azure OpenAI**: GPT-4 deployment
- **Cognitive Services**: Speech service

### CI/CD Pipeline

Automated deployment via GitHub Actions with:
- Federated credentials (OIDC, no secrets)
- Automatic build and test
- Azure App Service deployment
- Health check verification

**Trigger:** Push to `master` branch or manual workflow dispatch

### Manual Deployment

```powershell
# Deploy infrastructure
az deployment group create `
  --resource-group PoVicTranslate `
  --template-file infra/resources.bicep `
  --parameters environmentName=prod location=eastus2

# Publish and deploy application
dotnet publish src/Po.VicTranslate.Api/Po.VicTranslate.Api.csproj -c Release -o publish
az webapp deploy `
  --resource-group PoVicTranslate `
  --name PoVicTranslate `
  --src-path publish
```

---

## 📝 Documentation

- **[PRD.md](PRD.md)**: Complete Product Requirements Document
- **[CODE_COVERAGE.md](docs/CODE_COVERAGE.md)**: Test coverage analysis
- **[KQL Queries](docs/KQL/)**: Application Insights query library
- **[Diagrams](docs/diagrams/)**: Architecture and component diagrams

### Diagram Index

- **ProjectDependency**: Solution and package dependencies
- **ClassDiagram**: Domain entity relationships
- **SequenceDiagram**: Translation API call flow
- **Flowchart**: Translation use case workflow
- **ComponentHierarchy**: Blazor component structure

---

## 🧪 Development Workflow

### Code Quality

```powershell
# Build solution
dotnet build

# Run tests
dotnet test --verbosity minimal

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/*.cobertura.xml" -targetdir:"docs/coverage"

# Format code
dotnet format
```

### Debugging

- **API**: Press F5 in Visual Studio or `dotnet run --project src/Po.VicTranslate.Api`
- **Client**: Automatically hosted by API during development
- **Logs**: Check `log.txt` in repository root (development only)

---

## 📄 License

This project is licensed under the MIT License.

---

## 👥 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 🔗 Links

- **Production**: https://povictranslate.azurewebsites.net
- **Swagger**: https://povictranslate.azurewebsites.net/swagger
- **Repository**: https://github.com/punkouter26/PoVicTranslate
- **Azure Portal**: [App Service](https://portal.azure.com)

---

## 📞 Support

For issues, questions, or contributions, please open an issue on GitHub.

**Built with ❤️ using .NET 9.0 and Azure**
