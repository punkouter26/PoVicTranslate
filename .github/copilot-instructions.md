1. Core Principles
Framework: Build applications on the latest stable .NET version.
Frontend: Blazor WebAssembly hosted by a .NET API backend.
Specialized UI: For 2D/3D graphics or physics-heavy applications, use appropriate JavaScript libraries (e.g., THREE.js, Cannon.js) via CDN, invoked from Blazor.
Primary Pattern: Employ Vertical Slice Architecture. Each feature should be organized into a self-contained "slice" that contains all the logic required to implement it, from the API endpoint down to data access. This improves modularity and reduces coupling between features.
Architectural Principles: Within each slice, apply principles from Clean Architecture to ensure a strong separation of concerns. The goal is to isolate business logic (Domain and Application layers) from infrastructure concerns (e.g., databases, external APIs, UI). Common domain models and core application interfaces can be shared across slices in dedicated projects.
Automation-First: Use the command line (dotnet, az, gh) for all project setup, build, and deployment tasks to ensure reproducibility.
Code Quality: Adhere to SOLID principles and apply Gang of Four (GoF) design patterns where they add clear, demonstrable value. Code should be self-documenting whenever possible.
Conciseness: A C# or Razor file exceeding 500 lines should be considered a candidate for refactoring. This is a guideline to encourage adherence to the Single Responsibility Principle, not a strict rule.
Proactive Code Refactoring*: Regularly remove unused code and files to maintain a clean codebase. All deletions, like any other code change, must be done through a Pull Request, with the justification explained in the PR description. This replaces the need for a separate, manual approval process.
2. Project Setup & Structure
Naming Convention:
Solution: PoAppName
Projects: Must be prefixed with Po. (e.g., Po.AppName.Api, Po.AppName.Application.Core).
Initial Scaffolding:
Generate the project using dotnet new commands, drawing inspiration from established Vertical Slice and Clean Architecture templates.
Create a README.md file describing the application's purpose, architecture, and setup instructions.
Configure launchSettings.json to start and debug only the Po.AppName.Api project at https://localhost:5001 and http://localhost:5000.
Enable dotnet watch for hot reloading to improve development velocity.
Use reputable open-source libraries and APIs.
Reference external CSS and JS libraries via CDN for performance
3. Backend & API Design
API Documentation: Configure Swagger/OpenAPI from project inception for clear API documentation and interactive testing.
Endpoint Testing: Ensure API endpoints are easily testable with tools like curl or Postman to simulate user interactions without relying on the UI.
Exception Handling: Implement global exception handling middleware in the API. It must:
Use Serilog to log detailed exception data.
Return a standardized Problem Details (RFC 7807) response to the client for predictable error handling.
4. Frontend Design (Blazor)
UI Components: Start with built-in Blazor components. For advanced needs like data grids or charts, use a standardized component library such as Radzen.Blazor.
Page Title: Dynamically set the HTML <title> tag in the main layout to the solution name for brand consistency.
5. Data Persistence*
Primary Data Store: Use file-based storage (JSON files, text files) for simple data needs, following a structured approach for scalability and maintainability.
Flexibility: For features with more complex requirements (e.g., relational data, complex queries, transactional consistency), Azure SQL or Cosmos DB may be used with approval from the tech lead.
Data Access: Use System.Text.Json for JSON serialization and file I/O operations for data persistence.
Abstract data operations behind feature-specific interfaces (e.g., IProductDataService, ILyricsDataService) rather than a generic Repository Pattern. This approach allows methods to be tailored to specific data structures and access patterns.
File Organization: Store data files in a structured directory hierarchy under the Data folder or a dedicated data directory.
6. Logging & Diagnostics
Logging Framework: Implement Serilog with two sinks configured:
Console Sink: For real-time development feedback.
Rolling File Sink: Logs verbose-level details to /logs/log-.txt. Logs should roll daily or by file size to preserve historical data for debugging.
Health Checks:
Implement a /healthz API endpoint using .NET Health Check services.
Create a /diag page in the Blazor UI that calls the /healthz endpoint to display the connection status of all critical dependencies (database, external APIs, etc.).
7. Testing*
Framework: Use xUnit for all tests.
Project Structure: Organize tests into three dedicated projects:
Po.AppName.UnitTests
Po.AppName.IntegrationTests
Po.AppName.FunctionalTests
Test-Driven Workflow:
Start with Tests: Begin feature development by writing tests that define the desired behavior and success criteria. This could be a unit test for business logic or an integration test for a feature slice.
Implement Logic: Write the application code required to make the tests pass.
Refactor: Clean up the code while ensuring all tests continue to pass.
Implement UI: Once the API endpoint is functional and tested, implement the Blazor UI components.
Pull Request: A feature is considered complete when all tests are passing and the Pull Request has been reviewed and approved. The PR must pass all automated CI checks.
Test Responsibilities:
Unit Tests: Verify individual components or business logic in isolation.
Integration Tests: Test a complete vertical slice, including its interaction with file-based storage and external services.
Functional Tests: Target the live API endpoint with HTTP requests to validate the entire request/response pipeline, including serialization, authentication, and authorization.
8. Secrets & Configuration
Local Development: Store secrets and user-specific keys in appsettings.Development.json and use .NET's User Secrets Manager.
Azure Deployment: All production secrets, connection strings, and API keys must be stored in Azure App Service Settings Env Variables and/or appSettings.json..
9. Deployment to Azure
Tooling: Use the Azure Developer CLI (azd) for all deployment operations.
The Azure Resource Group name must match the solution name (e.g., PoAppName).
Deploy all application-specific resources to this group / Look in the PoShared resource group for shareable resrouces
Infrastructure (Bicep):
Run azd init to generate initial Bicep templates.
Modify main.bicep to deploy an App Service and any other required resources (e.g., Key Vault for secrets).
Configure the App Service to use a pre-existing, shared App Service Plan if available.
CI/CD:
Set up a GitHub Action to automatically build, test, and deploy the application to Azure on every push to the main branch.
Use the gh CLI to create the necessary repository secrets (AZURE_CREDENTIALS) required for the workflow.
