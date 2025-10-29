import { test } from '@playwright/test';

test('Azure App - Manual Inspection', async ({ page }) => {
  console.log('Opening Azure app...');
  console.log('Browser will stay open - check the Console tab for errors');
  console.log('Press Ctrl+C when done inspecting');
  
  await page.goto('https://povictranslate.azurewebsites.net/', {
    waitUntil: 'networkidle',
    timeout: 60000
  });

  // Keep the page open for 5 minutes
  await page.waitForTimeout(300000);
});
