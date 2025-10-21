import { test, expect } from '@playwright/test';

test('Azure App - Network Request Check', async ({ page }) => {
  const failedRequests: any[] = [];
  const allRequests: any[] = [];
  
  // Listen to all requests
  page.on('request', request => {
    allRequests.push({
      url: request.url(),
      method: request.method(),
      resourceType: request.resourceType()
    });
  });

  // Listen to failed requests
  page.on('requestfailed', request => {
    failedRequests.push({
      url: request.url(),
      method: request.method(),
      resourceType: request.resourceType(),
      failure: request.failure()?.errorText
    });
  });

  // Listen to console errors
  const consoleErrors: string[] = [];
  page.on('console', msg => {
    if (msg.type() === 'error') {
      consoleErrors.push(msg.text());
    }
  });

  // Listen to page errors (uncaught exceptions)
  page.on('pageerror', error => {
    console.log(`\n🚨 UNCAUGHT EXCEPTION: ${error.message}`);
    console.log(`Stack: ${error.stack}`);
  });

  console.log('Navigating to Azure app...');
  await page.goto('https://povictranslate.azurewebsites.net/', { 
    waitUntil: 'networkidle',
    timeout: 60000 
  });

  console.log('Waiting a few seconds...');
  await page.waitForTimeout(5000);

  console.log('\n=== FAILED REQUESTS ===');
  if (failedRequests.length > 0) {
    failedRequests.forEach(req => {
      console.log(`❌ ${req.method} ${req.url}`);
      console.log(`   Type: ${req.resourceType}, Error: ${req.failure}`);
    });
  } else {
    console.log('✓ No failed requests');
  }

  console.log('\n=== CONSOLE ERRORS ===');
  if (consoleErrors.length > 0) {
    consoleErrors.forEach(err => console.log(`❌ ${err}`));
  } else {
    console.log('✓ No console errors');
  }

  console.log('\n=== RESPONSE STATUS CODES ===');
  const responses = await Promise.all(
    allRequests.slice(0, 20).map(async req => {
      return `${req.method} ${req.url.substring(0, 100)}...`;
    })
  );
  responses.forEach(r => console.log(r));

  // Take screenshot
  await page.screenshot({ 
    path: 'azure-network-debug.png',
    fullPage: true 
  });
});
