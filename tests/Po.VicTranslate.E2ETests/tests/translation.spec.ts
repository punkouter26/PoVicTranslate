import { test, expect, Page } from '@playwright/test';

/**
 * Lean E2E smoke coverage.
 * Keep only tests that validate core functionality with low flake risk.
 */

async function waitForAppReady(page: Page) {
  await page.goto('/', { waitUntil: 'domcontentloaded' });
  await expect(page).toHaveTitle(/PoVicTranslate/i);
  await expect(page.getByPlaceholder(/enter text to translate/i)).toBeVisible({ timeout: 30000 });
  await expect(page.getByRole('button', { name: /^translate$/i })).toBeVisible({ timeout: 30000 });
}

test.describe('Smoke', () => {
  test('health endpoint is healthy', async ({ request }) => {
    const response = await request.get('/api/health');
    expect(response.status()).toBe(200);

    const body = await response.json();
    expect(body).toHaveProperty('Status');
    expect(body.Status).toBe('Healthy');
  });

  test('home page loads and shows Translate action', async ({ page }) => {
    await waitForAppReady(page);
    await expect(page.getByRole('button', { name: /^translate$/i })).toBeVisible();
  });
});
