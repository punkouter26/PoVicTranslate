import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  timeout: 60000, // 60 seconds for slower Blazor WASM loading
  use: {
    baseURL: 'http://localhost:5000', // Run tests locally only
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'mobile',
      use: { ...devices['Pixel 5'] },
    },
  ],

  // Web server configuration - starts the app before tests
  webServer: {
    command: 'dotnet run --project ../VictorianTranslator.Server/VictorianTranslator.Server.csproj',
    url: 'http://localhost:5000/health',
    reuseExistingServer: !process.env.CI,
    timeout: 120000, // 2 minutes to start
  },
});
