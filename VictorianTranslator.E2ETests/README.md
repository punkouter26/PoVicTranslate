# E2E Tests for Victorian Translator

This directory contains end-to-end tests using Playwright with TypeScript.

## Prerequisites

- Node.js 18+ installed
- npm or yarn installed
- .NET 9.0 SDK installed

## Setup

```bash
# Install dependencies
npm install

# Install Playwright browsers
npx playwright install chromium
```

## Running Tests

**Important**: Tests run against **localhost only** (http://localhost:5000). The Playwright configuration automatically starts the .NET server before running tests.

```bash
# Run all tests (automatically starts the server)
npm test

# Run tests in headed mode (see browser)
npm run test:headed

# Run tests in debug mode
npm run test:debug

# View test report
npm run report
```

The test runner will:
1. Start the VictorianTranslator.Server on http://localhost:5000
2. Wait for the /health endpoint to respond
3. Run all tests against the local server
4. Shut down the server when tests complete

## Test Structure

- `tests/translation.spec.ts` - Tests for translation and lyrics features
- `tests/health.spec.ts` - Tests for health endpoints and diagnostics

## Configuration

Tests are configured to run locally:
- Base URL: `http://localhost:5000`
- Timeout: 60 seconds (for Blazor WASM loading)
- Auto-start: Server starts automatically before tests
- Tests run on both Desktop Chrome and Mobile (Pixel 5)

## Writing New Tests

1. Create a new `.spec.ts` file in the `tests/` directory
2. Import test and expect from '@playwright/test'
3. Use `test.describe` for grouping related tests
4. Use `test()` for individual test cases

Example:
```typescript
import { test, expect } from '@playwright/test';

test.describe('My Feature', () => {
  test('should do something', async ({ page }) => {
    await page.goto('/');
    // Your test logic here
  });
});
```
