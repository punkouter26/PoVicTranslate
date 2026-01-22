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

    // Type test text and trigger change event
    await inputTextarea.fill('hello world');
    await inputTextarea.blur(); // Trigger blur to ensure change event fires
    
    // Wait a moment for Blazor to process the change
    await page.waitForTimeout(1000);

    // Click translate button - use the exact button (not nav buttons)
    const translateButton = page.getByRole('button', { name: 'Translate', exact: true });
    await expect(translateButton).toBeEnabled();
    await translateButton.click();

    // Wait for translation result to appear
    await page.waitForTimeout(4000);

    // Take a screenshot for debugging
    await page.screenshot({ path: 'test-results/translation-result.png' });

    // Check for translation result or error message
    const translationResult = page.locator('.translated-text');
    const errorMessage = page.locator('.rz-alert');
    
    const hasResult = await translationResult.count() > 0;
    const hasError = await errorMessage.count() > 0;
    
    console.log(`\n📊 Translation attempt results:`);
    console.log(`   - Translation result present: ${hasResult}`);
    console.log(`   - Error message present: ${hasError}`);
    
    if (hasError) {
      const errorText = await errorMessage.textContent();
      console.log(`   - Error text: "${errorText}"`);
    }
    
    if (hasResult) {
      await expect(translationResult).toBeVisible({ timeout: 2000 });
      const translatedText = await translationResult.textContent();
      console.log(`\n✅ Translation successful:`);
      console.log(`   Input: "hello world"`);
      console.log(`   Output: "${translatedText}"`);

      // Verify translation is not empty and different from input
      expect(translatedText).toBeTruthy();
      expect(translatedText).not.toBe('hello world');
    } else {
      // Print page content for debugging
      const pageContent = await page.content();
      console.log(`\n❌ No translation result found. Page HTML snippet:`);
      console.log(pageContent.substring(0, 500));
      expect(hasResult, 'Translation result should be visible').toBe(true);
    }

    // Report any console errors during translation
    if (consoleErrors.length > 0) {
      console.log('\n=== TRANSLATION CONSOLE ERRORS ===');
      consoleErrors.forEach(error => console.log(error));
    }

    expect(consoleErrors, `Found ${consoleErrors.length} console errors during translation`).toHaveLength(0);
  });
});
