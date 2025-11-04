# Development Standards & Agent Guidelines

This document defines the development standards and best practices for building .NET applications. Follow these guidelines when generating or modifying code.

---

## 1. Environment & Setup

### .NET SDK
- **Version**: Use .NET 9 exclusively
- **global.json**: Must be pinned to the 9.0.xxx SDK version

### Local Development Ports
- **HTTP**: Port 5000
- **HTTPS**: Port 5001
- Configure in `launchSettings.json`

### Storage
- **Default**: Azure Table Storage
- **Local Development**: Use Azurite for local storage emulation

### Secrets Management
- **Local Development**: Store all sensitive keys (connection strings, API keys) using .NET User Secrets manager
- **Azure Deployment**: Store keys in App Service Environment Variables
- **Never**: Commit secrets to source control

### CLI Philosophy
- Do **NOT** create PowerShell scripts for tasks that can be done in one line via CLI
- Avoid creating numerous one-shot `.ps1` scripts in the application source

---

## 2. Solution Structure

### Naming Convention
All projects, solutions, and storage tables **must** use the prefix: `Po.[AppName].*`

### Root Folder Structure
```
/src          - Application source code
/tests        - Test projects
/docs         - Documentation (README.md, PRD.md, diagrams, coverage reports)
/scripts      - Utility scripts (.ps1, .sh)
/infra        - Bicep YAML files for Azure resource deployment
```

### Source Projects (`/src`)

#### Standard Architecture
- **Po.[AppName].Api**: ASP.NET Core API (hosts Blazor WASM client)
- **Po.[AppName].Client**: Blazor WebAssembly project
- **Po.[AppName].Shared**: DTOs and models shared between API and Client

#### Onion Architecture (Only if specified)
- **Po.[AppName].Infrastructure**: Data access and external services
- **Po.[AppName].Domain**: Business logic and domain models

**Note**: Do **NOT** create separate Domain or Infrastructure projects unless explicitly specified.

### Test Projects (`/tests`)

1. **Po.[AppName].UnitTests**
   - Framework: xUnit
   - Purpose: Backend business logic testing

2. **Po.[AppName].Client.Tests**
   - Framework: bUnit with xUnit
   - Purpose: Blazor component testing

3. **Po.[AppName].IntegrationTests**
   - Framework: xUnit
   - Purpose: API endpoint testing

4. **Po.[AppName].E2ETests**
   - Framework: Playwright with TypeScript
   - Browsers: Chromium and mobile only

---

## 3. Architecture

### Primary Pattern
**Vertical Slice Architecture**: Organize code by feature, not by layer

### Philosophy
- Prioritize **simple, well-factored code**
- Slices should be **self-contained**
- Apply **SOLID principles** pragmatically
- Use **GoF (Gang of Four) patterns** and document their usage when applicable

### Recommended Tooling
Consider using the following when they improve code quality and debugging:
- **CQRS** (Command Query Responsibility Segregation)
- **MediatR** (for mediator pattern implementation)
- **Minimal APIs** (for lightweight API endpoints)
- **Polly** (for resilience and transient fault handling)
- **Microsoft.FluentUI.AspNetCore.Components** (for UI components)
- **OpenTelemetry** (for distributed tracing)
- **dotnet-monitor** (for on-demand diagnostics)

---

## 4. Backend (API) Rules

### API Documentation
- **Swagger**: Enable for all endpoints
- **HTTP Files**: Generate `.http` files for easy manual API testing

### Health Checks
Implement **mandatory** health check endpoints:
- **Readiness**: `/health/ready`
- **Liveness**: `/health/live`

### Logging & Telemetry

#### Logging
- **Framework**: Use **Serilog** for structured logging
- **Development Environment**: Write to Debug Console
- **Production Environment**: Write to Application Insights

#### Telemetry
Use **.NET OpenTelemetry (OTel) abstractions** for custom telemetry:
- **ActivitySource**: For custom traces
- **Meter**: For custom metrics
- Track main application events with custom telemetry

---

## 5. Frontend (Client) Rules

### UX Design
- **Mobile-First**: Design for portrait mode first
- **Desktop**: Must also look professional on desktop layouts
- **Responsive**: Layout must be fluid and touch-friendly
- **Accessibility**: Follow WCAG guidelines

### Component Library
1. **Primary**: Use **Blazor Fluent UI components** first
2. **Secondary**: Only use **Radzen.Blazor** library if clearly necessary for specific, complex requirements

---

## 6. Testing Strategy

### Workflow
Follow **TDD (Test-Driven Development)**: 
```
Red → Green → Refactor
```

### Unit Tests
- **Coverage**: Must cover all new backend business logic
- **Scope**: MediatR handlers, services, domain logic
- **Framework**: xUnit

### Component Tests
- **Coverage**: Must cover all new Blazor components
- **Framework**: bUnit with xUnit
- **Test Cases**:
  - Component rendering
  - User interactions (e.g., button clicks, form submissions)
  - State changes in isolation

### Integration Tests
- **Coverage**: Must have a "happy path" test for every new API endpoint
- **Isolation**: Run against isolated test database
- **Storage**: Use **Azurite in-memory persistence mode**
- **Cleanup**: Full setup and teardown required
- **Data**: No data shall persist between test runs

### End-to-End (E2E) Tests
- **Framework**: Playwright with TypeScript
- **Browsers**: Chromium and mobile configurations only
- **Scope**: Critical user journeys and workflows

---

## Summary Checklist

When creating or modifying code, ensure:

- [ ] .NET 9 SDK is used with pinned `global.json`
- [ ] Local ports are 5000 (HTTP) and 5001 (HTTPS)
- [ ] Secrets are in User Secrets (local) or Environment Variables (Azure)
- [ ] Projects follow `Po.[AppName].*` naming convention
- [ ] Vertical Slice Architecture is used
- [ ] Swagger is enabled for all API endpoints
- [ ] Health checks (readiness/liveness) are implemented
- [ ] Serilog is configured for structured logging
- [ ] OpenTelemetry is used for custom telemetry
- [ ] UI is mobile-first and uses Fluent UI components
- [ ] TDD workflow is followed (Red → Green → Refactor)
- [ ] Unit tests cover new business logic
- [ ] Component tests cover new Blazor components
- [ ] Integration tests use Azurite in-memory mode
- [ ] E2E tests use Playwright with Chromium/mobile only

---

**Last Updated**: November 4, 2025
