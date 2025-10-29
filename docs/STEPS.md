# Development Setup & Workflow Steps

## Prerequisites

- **.NET SDK 9.0.xxx** - **REQUIRED**: Builds will fail if a different SDK major version is used
- **PowerShell** (Windows) or **Bash** (Linux/macOS) for running scripts
- **Azurite** for local Azure Table Storage emulation
- **Visual Studio 2022** (recommended) or **VS Code** with C# extension

---

## Initial Setup

### 1. Verify .NET SDK Version

```powershell
dotnet --version
```

**Expected output**: `9.0.xxx` (where xxx is the patch version)

The `global.json` file at the repository root pins the SDK to version `9.0.306` with `rollForward: latestPatch`. This ensures:
- Only .NET 9 SDK is used
- Latest patch version is automatically adopted
- Builds fail if wrong major SDK version is detected

### 2. Restore Dependencies

```powershell
dotnet restore PoVicTranslate.sln
```

### 3. Install Azurite (if not already installed)

```powershell
npm install -g azurite
```

Or use the Azure Storage Emulator if you prefer.

### 4. Start Azurite for Local Development

```powershell
.\scripts\start-azurite.ps1
```

This starts Azurite on default ports for local Azure Table Storage emulation.

---

## Building & Running

### Build the Solution

```powershell
.\scripts\build.ps1
```

Or manually:

```powershell
dotnet build PoVicTranslate.sln
```

### Run the API Locally

```powershell
.\scripts\run-api.ps1
```

Or manually:

```powershell
cd src\Po.VicTranslate.Api
dotnet run
```

The API will bind to:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`

**REQUIRED**: These ports are enforced in `launchSettings.json`.

### Access Swagger/OpenAPI Documentation

Navigate to:
- Development: `https://localhost:5001/swagger`
- Production: `https://localhost:5001/swagger` (still available but not the default page)

### Verify Health Check Endpoint

**REQUIRED**: The mandatory health check endpoint is available at:

```powershell
curl https://localhost:5001/api/health
```

Expected response: `HTTP 200 OK` with health status information.

---

## Code Style & Formatting

### Enforce Code Style

**REQUIRED**: All code must pass `dotnet format` checks before committing.

Run the format script:

```powershell
.\scripts\format.ps1
```

This script:
1. Verifies no formatting changes are needed
2. Automatically applies formatting if issues are detected
3. Uses `.editorconfig` rules for consistency

### EditorConfig Rules

The `.editorconfig` file enforces:
- **.NET 9.0** coding standards
- **Max file length guideline**: 500 lines (Preferred rule)
- **Naming conventions**: `Po.[AppName].*` pattern
- **Code quality**: SOLID principles, performance, and security rules

### Pre-Commit Check

Before committing code, always run:

```powershell
.\scripts\format.ps1
```

**CI/CD gates**: Future CI pipelines will fail builds that don't pass `dotnet format --verify-no-changes`.

---

## Testing Workflow

### Test-Driven Development (TDD) Process

**REQUIRED**: Follow strict TDD workflow:

1. **Write a failing test** (Unit or Integration)
2. **Implement minimum code** to pass the test
3. **Refactor** code while keeping tests green
4. **Repeat** for each new feature or bug fix

### Running Unit Tests

```powershell
.\scripts\test.ps1
```

Or manually:

```powershell
dotnet test tests\Po.VicTranslate.UnitTests\Po.VicTranslate.UnitTests.csproj
```

**Test Framework**: xUnit v3 with FluentAssertions and Moq

**Coverage Requirements**:
- All new business logic (command/query handlers, domain services) must have unit tests
- Use Moq for mocking dependencies
- Use FluentAssertions for readable assertions

### Running Integration Tests

```powershell
dotnet test tests\Po.VicTranslate.IntegrationTests\Po.VicTranslate.IntegrationTests.csproj
```

**Requirements**:
- All new API endpoints must have at least one "happy path" integration test
- Tests must run against isolated database (Azurite or in-memory)
- Tests must include setup and teardown to ensure no data persists

**Example Integration Test**:

```csharp
[Fact]
public async Task ApiHealth_ShouldReturnOk_WhenServicesAreHealthy()
{
    // Act
    var response = await _client.GetAsync("/api/health", TestContext.Current.CancellationToken);

    // Assert
    response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
}
```

### Database Isolation for Integration Tests

Integration tests use:
- **Azurite** for local Azure Table Storage testing
- **Setup/Teardown**: Each test class creates and destroys its own test data
- **No shared state**: Tests must be independent and runnable in any order

### Running All Tests

```powershell
dotnet test PoVicTranslate.sln
```

### E2E Tests (Manual Execution Only)

**REQUIRED**: E2E tests use **Playwright with TypeScript (MCP)** and are **excluded from CI/CD**.

#### Setup E2E Tests

Navigate to the E2E tests directory:

```powershell
cd tests\Po.VicTranslate.E2ETests
```

Install Playwright dependencies:

```powershell
npm install
npx playwright install
```

#### Run E2E Tests Manually

```powershell
npx playwright test
```

**Note**: E2E tests are NOT included in automated CI/CD pipelines. They are intended for manual execution by developers during QA and before major releases.

#### View E2E Test Reports

```powershell
npx playwright show-report
```

---

## Project Structure & Naming

### Repository Layout

```
/
├── /docs              # Documentation (README.md, PRD.md, STEPS.md)
├── /scripts           # CLI helper scripts (.ps1, .sh)
├── /src               # Application source code
│   ├── Po.VicTranslate.Api       # ASP.NET Core API
│   ├── Po.VicTranslate.Client    # Blazor WASM
│   └── Po.VicTranslate.Shared    # Shared DTOs/Models
├── /tests             # Test projects
│   ├── Po.VicTranslate.UnitTests
│   ├── Po.VicTranslate.IntegrationTests
│   └── Po.VicTranslate.E2ETests  # TypeScript/Playwright
└── global.json        # SDK version pinning
```

### Naming Conventions

**REQUIRED**: All projects and database tables follow the `Po.[AppName].*` convention.

Examples:
- `Po.VicTranslate.Api`
- `Po.VicTranslate.Client`
- `Po.VicTranslate.Shared`
- Table: `PoVicTranslate[TableName]`

---

## Development Guidelines

### File Size Constraint

**Preferred Rule**: Limit files to **≤500 lines** to maintain simplicity and focus.

- Use linters/analyzers to enforce this limit
- `.editorconfig` marks violations as suggestions
- Refactor large files into smaller, well-factored modules

### Architecture Principles

1. **Primary**: Vertical Slice Architecture
2. **Secondary**: Clean Architecture (only when complexity requires separation)
3. **Philosophy**: Simple, small, well-factored code
4. **Patterns**: Apply SOLID and appropriate GoF patterns pragmatically

### Error Handling

**REQUIRED**: All errors must be returned as **RFC 7807 Problem Details**.

- Global exception handling middleware transforms all exceptions
- **Never** expose raw exception messages or stack traces in production
- Use Serilog for structured logging with full exception details

### Logging

**REQUIRED**: Use **Serilog** for structured logging.

Configuration:
- **Debug sink**: Rich text format for development
- **Console sink**: Standard output for production
- **File sink**: Local log files for debugging

### API Documentation

**REQUIRED**: Swagger/OpenAPI is enabled by default.

- All endpoints are automatically documented
- Generate `.http` files for easy endpoint testing
- Use Swagger UI for manual verification during development

---

## Common Commands

### Clean Build Artifacts

```powershell
.\scripts\clean.ps1
```

Or manually:

```powershell
dotnet clean PoVicTranslate.sln
```

### Update Namespaces (if needed)

```powershell
.\scripts\update-namespaces.ps1
```

### Run Tests with Coverage

```powershell
dotnet test --collect:"XPlat Code Coverage"
```

### Publish for Deployment

```powershell
dotnet publish src\Po.VicTranslate.Api\Po.VicTranslate.Api.csproj -c Release -o publish
```

---

## Troubleshooting

### SDK Version Mismatch

**Error**: Build fails with SDK version error

**Solution**: Ensure you have .NET 9 SDK installed and `global.json` points to `9.0.xxx`.

```powershell
dotnet --list-sdks
```

### Port Conflicts

**Error**: Ports 5000/5001 are already in use

**Solution**: Stop other processes using those ports or update `launchSettings.json` (not recommended).

### Azurite Not Running

**Error**: Integration tests fail with storage connection errors

**Solution**: Start Azurite before running tests:

```powershell
.\scripts\start-azurite.ps1
```

### Format Check Fails in CI

**Error**: CI pipeline fails on `dotnet format --verify-no-changes`

**Solution**: Run format script locally before committing:

```powershell
.\scripts\format.ps1
```

---

## CI/CD Automation Checks

Future CI pipelines will enforce:

1. **.NET SDK major version** is 9
2. **Required ports** are configured (5000, 5001)
3. **Project prefix** conforms to `Po.[AppName].*`
4. **/api/health endpoint** exists
5. **Problem Details middleware** is present
6. **dotnet format** passes with no changes
7. **All unit and integration tests** pass
8. **Build succeeds** for Release configuration

---

## Additional Resources

- **README.md**: Project overview, key features, architecture
- **PRD.md**: Product requirements, UI pages, technical specifications
- **.editorconfig**: Code style rules and enforcement
- **global.json**: SDK version pinning configuration

---

## Quick Reference

| Command | Purpose |
|---------|---------|
| `.\scripts\build.ps1` | Build the solution |
| `.\scripts\run-api.ps1` | Run the API locally |
| `.\scripts\test.ps1` | Run unit tests |
| `.\scripts\format.ps1` | Check and apply code formatting |
| `.\scripts\clean.ps1` | Clean build artifacts |
| `.\scripts\start-azurite.ps1` | Start Azurite for local storage |
| `dotnet test` | Run all tests |
| `dotnet restore` | Restore NuGet packages |
| `curl https://localhost:5001/api/health` | Verify health endpoint |
| `npx playwright test` | Run E2E tests (manual) |

---

**Remember**: Always follow TDD, enforce code style with `dotnet format`, and ensure all tests pass before committing!
