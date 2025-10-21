import { test, expect } from '@playwright/test';

test('Azure App - Console Error Check', async ({ page }) => {
  const consoleMessages: string[] = [];
  const consoleErrors: string[] = [];
  
  // Listen to console messages
  page.on('console', msg => {
    const text = msg.text();
    consoleMessages.push(`[${msg.type()}] ${text}`);
    if (msg.type() === 'error') {
      consoleErrors.push(text);
    }
  });

  // Listen to page errors
  page.on('pageerror', error => {
    consoleErrors.push(`PAGE ERROR: ${error.message}\n${error.stack}`);
  });

  console.log('Navigating to Azure app...');
  await page.goto('https://povictranslate.azurewebsites.net/', { 
    waitUntil: 'domcontentloaded',
    timeout: 60000 
  });

  console.log('Waiting for page to settle...');
  await page.waitForTimeout(5000);

  console.log('\n=== CONSOLE MESSAGES ===');
  consoleMessages.forEach(msg => console.log(msg));

  console.log('\n=== CONSOLE ERRORS ===');
  if (consoleErrors.length > 0) {
    consoleErrors.forEach(err => console.log(err));
  } else {
    console.log('No console errors found during page load');
  }

  // Now try to interact and see what errors occur
  console.log('\n=== ATTEMPTING TRANSLATION ===');
  
  const inputField = page.locator('textarea').first();
  await inputField.waitFor({ state: 'visible', timeout: 10000 });
  await inputField.fill('Test text');
  
  const translateButton = page.locator('button:has-text("Transform to Victorian")').first();
  await translateButton.click();
  
  console.log('Clicked translate button, waiting for errors...');
  await page.waitForTimeout(3000);

  console.log('\n=== ERRORS AFTER TRANSLATION CLICK ===');
  if (consoleErrors.length > 0) {
    console.log('Found errors:');
    consoleErrors.forEach(err => console.log(err));
  } else {
    console.log('No errors after translation click');
  }

  // Take screenshot
  await page.screenshot({ 
    path: 'azure-console-debug.png',
    fullPage: true 
  });

  console.log('\n=== SUMMARY ===');
  console.log(`Total console messages: ${consoleMessages.length}`);
  console.log(`Total errors: ${consoleErrors.length}`);
});
