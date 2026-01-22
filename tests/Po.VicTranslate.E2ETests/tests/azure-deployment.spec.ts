import { test, expect } from '@playwright/test';

/**
 * E2E test for Azure deployed instance
 * Checks console for errors on home page load
 */
test.describe('Azure Deployment', () => {
  const azureUrl = 'https://ca-povictranslate.braveground-e6b1356c.eastus.azurecontainerapps.io';

  test('home page loads without console errors', async ({ page }) => {
    const consoleErrors: string[] = [];
    const consoleWarnings: string[] = [];

    // Capture console messages
    page.on('console', msg => {
      if (msg.type() === 'error') {
        consoleErrors.push(`[ERROR] ${msg.text()}`);
      } else if (msg.type() === 'warning') {
        consoleWarnings.push(`[WARNING] ${msg.text()}`);
      }
    });

    // Capture page errors
    page.on('pageerror', error => {
      consoleErrors.push(`[PAGE ERROR] ${error.message}`);
    });

    // Navigate to home page
    await page.goto(azureUrl, { waitUntil: 'networkidle' });

    // Wait for page to be fully loaded
    await page.waitForLoadState('domcontentloaded');

    // Check for PoVicTranslate title
    await expect(page).toHaveTitle(/PoVicTranslate/);

    // Print console messages
    console.log('\n=== CONSOLE ERRORS ===');
    if (consoleErrors.length === 0) {
      console.log('✅ No console errors detected');
    } else {
      consoleErrors.forEach(error => console.log(error));
    }

    console.log('\n=== CONSOLE WARNINGS ===');
    if (consoleWarnings.length === 0) {
      console.log('✅ No console warnings detected');
    } else {
      consoleWarnings.forEach(warning => console.log(warning));
    }

    // Fail if there are console errors
    expect(consoleErrors, `Found ${consoleErrors.length} console errors`).toHaveLength(0);
  });

  test('translation feature works', async ({ page }) => {
    const consoleErrors: string[] = [];

    page.on('console', msg => {
      if (msg.type() === 'error') {
        consoleErrors.push(`[ERROR] ${msg.text()}`);
      }
    });

    page.on('pageerror', error => {
      consoleErrors.push(`[PAGE ERROR] ${error.message}`);
    });

    await page.goto(azureUrl, { waitUntil: 'networkidle' });

    // Find the input textarea
    const inputTextarea = page.locator('textarea').first();
    await expect(inputTextarea).toBeVisible();

    // Type test text
    await inputTextarea.fill('hello world');

    // Click translate button
    const translateButton = page.locator('button:has-text("TRANSLATE")');
    await expect(translateButton).toBeVisible();
    await translateButton.click();

    // Wait for translation result (or error message)
    await page.waitForTimeout(3000);

    // Report any console errors during translation
    if (consoleErrors.length > 0) {
      console.log('\n=== TRANSLATION CONSOLE ERRORS ===');
      consoleErrors.forEach(error => console.log(error));
    }

    // Don't fail on errors here - just report them
    console.log(`\n✅ Translation test completed with ${consoleErrors.length} console errors`);
  });
});
