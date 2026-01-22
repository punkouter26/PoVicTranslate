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
    // Using Aspire default health endpoint path
    const response = await request.get('/health');
    expect(response.status()).toBe(200);

    // Aspire health checks return plain text "Healthy"
    const body = await response.text();
    expect(body).toContain('Healthy');
  });

  test('home page loads and shows Translate action', async ({ page }) => {
    await waitForAppReady(page);
    await expect(page.getByRole('button', { name: /^translate$/i })).toBeVisible();
  });

  test('translates "what is up?" to Victorian language', async ({ page }) => {
    await waitForAppReady(page);

    // Enter the text to translate
    const inputField = page.getByPlaceholder(/enter text to translate/i);
    await inputField.click();
    await inputField.fill('what is up?');
    
    // Trigger change event to enable the button
    await inputField.press('Tab');

    // Wait for translate button to be enabled
    const translateButton = page.getByRole('button', { name: /^translate$/i });
    await expect(translateButton).toBeEnabled({ timeout: 10000 });

    // Click the translate button
    await translateButton.click();

    // Wait for the results card with translated text to appear
    const translatedOutput = page.locator('p.translated-text');
    await expect(translatedOutput).toBeVisible({ timeout: 60000 });

    // Verify we got a response (Victorian English should differ from modern)
    const translatedText = await translatedOutput.textContent();
    expect(translatedText).toBeTruthy();
    expect(translatedText!.length).toBeGreaterThan(0);
    
    // The translation should not be exactly the same as input
    expect(translatedText!.toLowerCase()).not.toBe('what is up?');
  });
});
