# E2E Tests for Po.VicTranslate# E2E Tests for Victorian Translator



**REQUIRED**: End-to-end tests using **Playwright with TypeScript (MCP)**.This directory contains end-to-end tests using Playwright with TypeScript.



**Important**: These tests are **manual execution only** and are **excluded from automated CI/CD pipelines**.## Prerequisites



## Prerequisites- Node.js 18+ installed

- npm or yarn installed

- **Node.js 18+** installed- .NET 9.0 SDK installed

- **npm** or **yarn** installed

- **.NET 9.0 SDK** installed (enforced by `global.json`)## Setup

- **Po.VicTranslate.Api** project builds successfully

```bash

## Setup# Install dependencies

npm install

```bash

# Install dependencies# Install Playwright browsers

npm installnpx playwright install chromium

```

# Install Playwright browsers

npx playwright install chromium## Running Tests

```

**Important**: Tests run against **localhost only** (http://localhost:5000). The Playwright configuration automatically starts the .NET server before running tests.

## Running Tests

```bash

**REQUIRED**: Tests run against **localhost only** on ports **HTTP 5000** and **HTTPS 5001**.# Run all tests (automatically starts the server)

npm test

The Playwright configuration automatically:

1. Starts the `Po.VicTranslate.Api` server on `http://localhost:5000`# Run tests in headed mode (see browser)

2. Waits for the `/api/health` endpoint to respond (health check validation)npm run test:headed

3. Runs all tests against the local server

4. Shuts down the server when tests complete# Run tests in debug mode

npm run test:debug

### Commands

# View test report

```bashnpm run report

# Run all tests (automatically starts the server)```

npm test

The test runner will:

# Run tests in headed mode (see browser)1. Start the VictorianTranslator.Server on http://localhost:5000

npm run test:headed2. Wait for the /health endpoint to respond

3. Run all tests against the local server

# Run tests in debug mode (step through tests)4. Shut down the server when tests complete

npm run test:debug

## Test Structure

# View HTML test report

npm run report- `tests/translation.spec.ts` - Tests for translation and lyrics features

```- `tests/health.spec.ts` - Tests for health endpoints and diagnostics



## Test Projects## Configuration



Tests run on two configurations to ensure **mobile-first, portrait-mode UX**:Tests are configured to run locally:

- Base URL: `http://localhost:5000`

1. **Desktop Chrome** - Standard desktop experience- Timeout: 60 seconds (for Blazor WASM loading)

2. **Mobile Portrait (Pixel 5)** - 393x851 viewport to validate mobile layout- Auto-start: Server starts automatically before tests

- Tests run on both Desktop Chrome and Mobile (Pixel 5)

**REQUIRED**: All main user flows must be tested on both desktop and narrow-screen mobile emulation.

## Writing New Tests

## Test Structure

1. Create a new `.spec.ts` file in the `tests/` directory

```2. Import test and expect from '@playwright/test'

tests/3. Use `test.describe` for grouping related tests

├── translation.spec.ts   # Translation and lyrics features4. Use `test()` for individual test cases

├── health.spec.ts        # Health endpoints and diagnostics

└── *.spec.ts             # Additional test suitesExample:

``````typescript

import { test, expect } from '@playwright/test';

## Configuration

test.describe('My Feature', () => {

- **Base URL**: `http://localhost:5000` (REQUIRED)  test('should do something', async ({ page }) => {

- **Health Check**: `/api/health` (REQUIRED endpoint)    await page.goto('/');

- **Timeout**: 60 seconds (for Blazor WASM loading)    // Your test logic here

- **Auto-start**: Server starts automatically before tests  });

- **Mobile Priority**: Tests include mobile portrait viewport});

```

## Writing New Tests

**TDD Workflow**: Follow the same TDD principles as unit/integration tests:

1. Write a failing E2E test
2. Implement the feature
3. Verify the test passes
4. Refactor

### Example Test

Create a new `.spec.ts` file in the `tests/` directory:

```typescript
import { test, expect } from '@playwright/test';

test.describe('Translation Feature', () => {
  test('should translate modern text to Victorian English', async ({ page }) => {
    // Navigate to main page
    await page.goto('/');

    // Enter text
    await page.fill('textarea[placeholder*="Type or paste"]', 'Hello world');

    // Click translate button
    await page.click('button:has-text("Make It Victorian")');

    // Verify translation appears
    await expect(page.locator('.translation-output')).toBeVisible();
  });
});
```

## Mobile-First Testing

**REQUIRED**: Verify responsive layout and touch-friendly controls on mobile:

```typescript
test.describe('Mobile UX', () => {
  test.use({ viewport: { width: 393, height: 851 } }); // Mobile portrait

  test('should be responsive on mobile portrait', async ({ page }) => {
    await page.goto('/');
    
    // Verify mobile-friendly layout
    const button = page.locator('button:has-text("Make It Victorian")');
    await expect(button).toBeVisible();
    
    // Verify touch-friendly size (min 44x44 pixels)
    const box = await button.boundingBox();
    expect(box?.width).toBeGreaterThanOrEqual(44);
    expect(box?.height).toBeGreaterThanOrEqual(44);
  });
});
```

## Manual Execution Only

**Important**: E2E tests are **NOT** included in:
- `dotnet test` commands
- CI/CD pipelines
- Automated test runs

Developers must run E2E tests **manually** during:
- QA processes
- Before major releases
- Feature validation

## Troubleshooting

### Port Already in Use

**Error**: `Error: listen EADDRINUSE: address already in use :::5000`

**Solution**: Stop any running instances of the API:

```powershell
# Windows PowerShell
Get-Process -Name "dotnet" | Stop-Process -Force
```

### Server Start Timeout

**Error**: `Error: Timed out waiting 120s`

**Solution**: Build the API project first:

```powershell
cd ../../src/Po.VicTranslate.Api
dotnet build
```

### Playwright Browsers Not Installed

**Error**: `Error: browserType.launch: Executable doesn't exist`

**Solution**: Install Playwright browsers:

```bash
npx playwright install chromium
```

## Additional Resources

- [Playwright Documentation](https://playwright.dev/)
- [Playwright TypeScript Guide](https://playwright.dev/docs/test-typescript)
- Main project documentation: `../../docs/README.md`
- Development workflow: `../../docs/STEPS.md`
