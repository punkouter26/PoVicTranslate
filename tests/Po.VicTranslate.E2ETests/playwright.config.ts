import { defineConfig, devices } from '@playwright/test';

/**
 * REQUIRED: E2E tests with Playwright TypeScript (MCP)
 * Manual execution only - excluded from automated CI/CD pipeline
 */
export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  timeout: 60000, // 60 seconds for slower Blazor WASM loading
  use: {
    baseURL: 'http://localhost:5002', // Use different port to avoid conflicts
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'mobile-portrait',
      use: { 
        ...devices['Pixel 5'],
        // REQUIRED: Mobile portrait UX priority
        viewport: { width: 393, height: 851 }
      },
    },
  ],

  // Web server configuration - starts the app before tests
  webServer: {
    command: 'dotnet run --project ../../src/Po.VicTranslate.Api/Po.VicTranslate.Api.csproj --launch-profile e2e',
    url: 'http://localhost:5002/api/health/live', // Use different port
    reuseExistingServer: !process.env.CI,
    timeout: 120000, // 2 minutes to start
  },
});
