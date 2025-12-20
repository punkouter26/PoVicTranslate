# PoVicTranslate - AI Agent Instructions

## 1. Project Identity & SDK Standards
- **Unified ID**: `PoVicTranslate` for Azure resources, `Po.VicTranslate.*` for namespaces
- **SDK**: .NET 10 exclusively, pinned in `global.json`
- **C# 14+**: Primary constructors, collection expressions, `field` keyword, `<IsAotCompatible>true</IsAotCompatible>`, `TreatWarningsAsErrors`
- **Packages**: Central management via `Directory.Packages.props`

## 2. Architecture & Logic Flow
- **Vertical Slice**: Feature folders with Minimal API endpoint + DTOs + Business Logic self-contained
- **Result Pattern**: `ErrorOr` library with `.Match()` returning `TypedResults` (200 OK / 400 BadRequest)
- **Safety**: `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>` mandatory

### Minimal API Pattern (.NET 10)
```csharp
// Use TypedResults with union types for explicit response contracts
app.MapGet("/todo/{id}", Results<Ok<Todo>, NotFound, BadRequest> (int id) => id switch
{
    <= 0 => TypedResults.BadRequest(),
    >= 1 and <= 10 => TypedResults.Ok(new Todo(id, "Task")),
    _ => TypedResults.NotFound()
});

// POST with model binding
app.MapPost("/todos", (TodoBindable todo) => todo);
```

### Current Structure
```
src/Po.VicTranslate.Api/     → ASP.NET Core hosts Blazor WASM
├── Controllers/             → Translation, Lyrics, Speech, Health
├── Services/                → Business logic (Template Method pattern)
├── Services/Validation/     → SOLID validators (IInputValidator, IDiagnosticValidator)
└── HealthChecks/            → Azure OpenAI, Speech, Internet connectivity
```

## 3. Data & Mapping
- **Mapperly**: Source-generated DTO-to-Entity mapping (compile-time safe)
- **Azure Table Storage**: Lightweight expression visitors for DTO filters
- **Resilience**: Polly pipelines (Retry + jitter, Circuit Breaker) on storage/HTTP clients

## 4. Authentication & UI (BFF Pattern)
- **BFF**: API proxies security for Blazor WASM; client uses Secure Cookies only (no JWTs)
- **Components**: Smart (Pages fetch data) vs Dumb (presentational children)
- **State**: Scoped `StateContainer` with `OnChange` event; see `TranslationViewModel`
- **Hydration**: `[PersistentComponentState]` for SSR→WASM transitions

## 5. Infrastructure & DevOps
- **Bicep**: `infra/main.bicep` (subscription-scoped) + `resources.bicep` (App Service Linux)
- **Secrets**: Azure Key Vault via `DefaultAzureCredential`; Azurite locally
- **Config**: `appsettings.json` = non-sensitive only; secrets via Key Vault References
- **Deployment**: Use `azd up` (validate with `azd provision --preview` first)

## 6. Health & Monitoring
- **Endpoints**: `/api/health` (full), `/api/health/live` (liveness), `/api/health/ready` (readiness)
- **Logging**: Serilog → Console (dev) / Application Insights (prod), Information+ in prod
- **Telemetry**: OpenTelemetry → Azure Monitor; custom metrics via `ICustomTelemetryService`

## 7. Development Workflow

### Commands
```powershell
# Run API (serves Blazor client)
dotnet run --project src/Po.VicTranslate.Api

# Tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# E2E (Playwright + Chromium)
cd tests/Po.VicTranslate.E2ETests
$env:DISABLE_HTTPS_REDIRECTION="true"; npx playwright test
```

### Configuration (User Secrets)
```powershell
cd src/Po.VicTranslate.Api
dotnet user-secrets set "ApiSettings:AzureOpenAIEndpoint" "https://..."
dotnet user-secrets set "ApiSettings:AzureOpenAIApiKey" "..."
dotnet user-secrets set "ApiSettings:AzureOpenAIDeploymentName" "gpt-4"
dotnet user-secrets set "ApiSettings:AzureSpeechRegion" "eastus2"
dotnet user-secrets set "ApiSettings:AzureSpeechSubscriptionKey" "..."
```

### Testing Strategy
| Type | Framework | Pattern |
|------|-----------|---------|
| Unit | xUnit + Moq + FluentAssertions | TDD: Red → Green → Refactor |
| Integration | WebApplicationFactory + Azurite | Happy path per endpoint |
| E2E | Playwright TypeScript | Chromium + mobile emulators |

### Ports
- HTTP: 5000 / HTTPS: 5001 (dev)
- E2E: 5002 with `DISABLE_HTTPS_REDIRECTION=true`

## 8. Azure Best Practices
- **Authentication**: Use Managed Identity for Azure-hosted resources; never hardcode credentials
- **Error Handling**: Implement retry with exponential backoff for transient failures
- **Security**: Use Key Vault for secrets; disable key-based access for storage/Cosmos
- **IaC**: Place Bicep files in `infra/`; use latest API versions; validate with `what-if`
- **Monitoring**: Log Information+ in production; implement circuit breakers for external calls


