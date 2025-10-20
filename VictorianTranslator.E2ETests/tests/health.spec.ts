import { test, expect } from '@playwright/test';

test.describe('Health and Diagnostics Endpoints', () => {
  test('health endpoint should be accessible', async ({ page }) => {
    const response = await page.goto('/health');
    
    expect([200, 503]).toContain(response?.status());
  });

  test('healthz endpoint should be accessible', async ({ page }) => {
    const response = await page.goto('/healthz');
    
    expect([200, 503]).toContain(response?.status());
  });

  test('diagnostics page should load', async ({ page }) => {
    await page.goto('/diag', { waitUntil: 'domcontentloaded' });
    
    // Wait for Blazor app to load
    await page.waitForSelector('body', { timeout: 60000 });
    
    // Verify page loaded
    const content = await page.content();
    expect(content.length).toBeGreaterThan(50);
  });
});

test.describe('API Swagger Documentation', () => {
  test('swagger UI should be accessible', async ({ page }) => {
    await page.goto('/swagger', { waitUntil: 'domcontentloaded' });
    
    // Wait for page content
    await page.waitForSelector('body', { timeout: 60000 });
    
    // Verify page loaded
    const content = await page.content();
    expect(content.length).toBeGreaterThan(0);
  });
});
