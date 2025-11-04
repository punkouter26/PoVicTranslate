import { test, Page } from '@playwright/test';

// Helper function to wait for Blazor app to be ready
async function waitForBlazorApp(page: Page) {
  await page.waitForSelector('#app:not(:empty)', { timeout: 30000 });
  try {
    await page.waitForSelector('.main-wrapper, .urban-container, body > div', { timeout: 10000 });
  } catch {
    console.log('Main selectors not found, waiting for basic hydration...');
  }
  try {
    await page.waitForSelector('.loading-overlay', { state: 'hidden', timeout: 15000 });
  } catch {
    // Loading overlay might not be present
  }
  await page.waitForTimeout(2000);
}

test('capture page screenshot for analysis', async ({ page }) => {
  console.log('Navigating to home page...');
  await page.goto('/', { waitUntil: 'domcontentloaded' });
  
  console.log('Waiting for Blazor app...');
  await waitForBlazorApp(page);
  
  console.log('Taking full page screenshot...');
  await page.screenshot({ 
    path: 'page-analysis-full.png', 
    fullPage: true 
  });
  
  console.log('Taking viewport screenshot...');
  await page.screenshot({ 
    path: 'page-analysis-viewport.png'
  });
  
  // Get page content for analysis
  const bodyText = await page.locator('body').textContent();
  console.log('=== PAGE TEXT CONTENT ===');
  console.log(bodyText);
  console.log('=========================');
  
  // Get HTML structure
  const html = await page.content();
  console.log('=== PAGE HTML (first 2000 chars) ===');
  console.log(html.substring(0, 2000));
  console.log('====================================');
  
  // Check for specific elements
  const textareaExists = await page.locator('#inputText').count();
  const buttonExists = await page.locator('button:has-text("Transform to Victorian")').count();
  const mainWrapperExists = await page.locator('.main-wrapper').count();
  const urbanContainerExists = await page.locator('.urban-container').count();
  
  console.log('=== ELEMENT COUNT ===');
  console.log('Textarea (#inputText):', textareaExists);
  console.log('Transform Button:', buttonExists);
  console.log('Main Wrapper:', mainWrapperExists);
  console.log('Urban Container:', urbanContainerExists);
  console.log('=====================');
});
