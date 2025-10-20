# PoVicTranslate - AI Coding Assistant Instructions

## Project Overview
PoVicTranslate is a Victorian English Translator with Speech Synthesis, built on .NET 9.0 using Blazor WebAssembly hosted by a .NET API backend.

## Core Principles
1. **Framework**: .NET 9.0
2. **Architecture**: Vertical Slice Architecture with Clean Architecture principles
3. **Frontend**: Blazor WebAssembly
4. **Backend**: ASP.NET Core Web API
5. **Deployment**: Azure App Service (F1 tier, shared plan)

## Coding Standards
- Follow SOLID principles
- Apply Gang of Four design patterns where they add value
- Keep files under 500 lines when possible
- Use nullable reference types
- Enable implicit usings
- Code should be self-documenting

## Project Structure
- **VictorianTranslator.Server**: ASP.NET Core backend API
  - Controllers: API endpoints
  - Services: Business logic with interface-based design
  - Models: DTOs and domain models
  - Data: File-based JSON storage
  
- **VictorianTranslator.Client**: Blazor WebAssembly frontend
  - Components: Reusable Blazor components
  - Pages: Routable page components
  - Services: Client-side services
  - ViewModels: Page-specific view models

## Configuration
- Local development uses `appsettings.Development.json`
- Production uses `appsettings.json` + Azure App Service environment variables
- Secrets are stored in appsettings files (private repo)
- Required Azure services:
  - Azure OpenAI (for translation)
  - Azure Speech Services (for speech synthesis)
  - Application Insights (for telemetry)

## Testing
- Use xUnit for all tests
- Three test projects:
  - Unit Tests
  - Integration Tests
  - Functional Tests
- Follow test-driven development workflow

## Logging & Diagnostics
- Serilog for structured logging
- Application Insights for telemetry
- Log levels: Information and Warning
- Health check endpoint: `/health` or `/healthz`
- Diagnostics page: `/diag`

## Deployment
- Resource Group: `PoVicTranslate`
- Region: `eastus2`
- App Service Plan: Use existing plan in `PoShared` resource group
- CI/CD: GitHub Actions triggered on push to master
- Use `azd` CLI for infrastructure deployment

## PWA Support
- Manifest and service worker configured
- Optimized for mobile portrait mode
- Offline support for essential features

## Development Workflow
1. Always build and verify before committing
2. Fix all warnings and errors
3. Use `dotnet format` for consistent styling
4. Keep NuGet packages up to date
5. Debug with F5 using `.vscode/launch.json`

## Best Practices
- Implement graceful degradation for AI service failures
- Enable Swagger in all environments
- Use Blazor WebAssembly hosted pattern (no CORS needed)
- Abstract external dependencies behind interfaces
- Validate all configurations on startup
