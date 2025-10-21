import { test, expect } from '@playwright/test';

test('Azure App - Diagnostic Check', async ({ page }) => {
  console.log('Navigating to Azure app...');
  await page.goto('https://povictranslate.azurewebsites.net/', { 
    waitUntil: 'domcontentloaded',
    timeout: 60000 
  });

  console.log('Waiting a few seconds for Blazor...');
  await page.waitForTimeout(10000);

  // Get all text on the page
  const bodyText = await page.locator('body').textContent();
  console.log('=== PAGE TEXT ===');
  console.log(bodyText);
  console.log('=================');

  // Get all buttons
  const buttons = await page.locator('button').allTextContents();
  console.log('=== BUTTONS ===');
  console.log(buttons);
  console.log('===============');

  // Take a screenshot
  await page.screenshot({ 
    path: 'azure-diagnostic.png',
    fullPage: true 
  });

  console.log('Screenshot saved to: azure-diagnostic.png');

  // Check for errors
  const hasError = await page.locator('text=/error|500|failed/i').count();
  console.log(`Error elements found: ${hasError}`);
});
