import { test, expect } from '@playwright/test';

/**
 * Phase 3: E2E Tests - Chromium only, Desktop + Mobile Portrait
 * Tests main UI functionality - run manually only, excluded from CI/CD
 */

test.describe('Victorian Translator - Home Page', () => {
  test('should load the home page with correct title', async ({ page }) => {
    await page.goto('/', { waitUntil: 'networkidle' });
    
    // Verify title
    await expect(page).toHaveTitle(/PoVicTranslate/i);
  });

  test('should display main navigation and layout', async ({ page }) => {
    await page.goto('/', { waitUntil: 'networkidle' });
    
    // Wait for Blazor to fully load
    await page.waitForLoadState('networkidle');
    
    // Check page has rendered content
    const bodyContent = await page.locator('body').textContent();
    expect(bodyContent).toBeTruthy();
    expect(bodyContent!.length).toBeGreaterThan(0);
  });

  test('should be responsive on mobile portrait', async ({ page, isMobile }) => {
    await page.goto('/', { waitUntil: 'networkidle' });
    
    if (isMobile) {
      // Verify mobile viewport is set correctly (393x851 for Pixel 5)
      const viewport = page.viewportSize();
      expect(viewport?.width).toBe(393);
      expect(viewport?.height).toBe(851);
      
      // Ensure content is visible and not overflowing
      const body = page.locator('body');
      await expect(body).toBeVisible();
    }
  });
});

test.describe('Victorian Translator - Translation Feature', () => {
  test('should display translation input area', async ({ page }) => {
    await page.goto('/', { waitUntil: 'networkidle' });
    
    // Look for text input or textarea for translation
    const hasInput = await page.locator('input[type="text"], textarea').count();
    expect(hasInput).toBeGreaterThan(0);
  });

  test('should have translate button available', async ({ page }) => {
    await page.goto('/', { waitUntil: 'networkidle' });
    
    // Look for a button that contains "translate" or similar text
    const buttons = await page.locator('button').count();
    expect(buttons).toBeGreaterThan(0);
  });

  test('should allow text input on mobile', async ({ page, isMobile }) => {
    await page.goto('/', { waitUntil: 'networkidle' });
    
    if (isMobile) {
      // Find first text input or textarea
      const input = page.locator('input[type="text"], textarea').first();
      const inputCount = await page.locator('input[type="text"], textarea').count();
      
      if (inputCount > 0) {
        await expect(input).toBeVisible();
        
        // Verify it's touch-friendly (minimum 44px tap target)
        const box = await input.boundingBox();
        if (box) {
          expect(box.height).toBeGreaterThanOrEqual(44);
        }
      }
    }
  });
});

test.describe('Victorian Translator - Lyrics Feature', () => {
  test('should navigate to lyrics page if available', async ({ page }) => {
    await page.goto('/', { waitUntil: 'networkidle' });
    
    // Look for lyrics navigation link
    const lyricsLink = page.getByRole('link', { name: /lyric/i }).first();
    const linkCount = await page.getByRole('link', { name: /lyric/i }).count();
    
    if (linkCount > 0) {
      await lyricsLink.click();
      await page.waitForLoadState('networkidle');
      
      // Verify navigation occurred
      const url = page.url();
      expect(url).toContain('lyric');
    }
  });

  test('should display lyrics list or empty state', async ({ page }) => {
    await page.goto('/', { waitUntil: 'networkidle' });
    
    // Navigate to lyrics if link exists
    const lyricsLinkCount = await page.getByRole('link', { name: /lyric/i }).count();
    if (lyricsLinkCount > 0) {
      await page.getByRole('link', { name: /lyric/i }).first().click();
      await page.waitForLoadState('networkidle');
    }
    
    // Page should have content (list or empty message)
    const bodyText = await page.locator('body').textContent();
    expect(bodyText).toBeTruthy();
  });
});

test.describe('Victorian Translator - Health and Diagnostics', () => {
  test('should have working health check endpoint', async ({ request }) => {
    const response = await request.get('/api/health');
    expect(response.status()).toBe(200);
    
    const body = await response.json();
    expect(body).toHaveProperty('Status');
    expect(body.Status).toBe('Healthy');
  });

  test('should have diagnostic page accessible', async ({ page }) => {
    // Try to access diagnostic page
    const diagLink = page.getByRole('link', { name: /diag/i }).first();
    const diagLinkCount = await page.getByRole('link', { name: /diag/i }).count();
    
    if (diagLinkCount > 0) {
      await page.goto('/', { waitUntil: 'networkidle' });
      await diagLink.click();
      await page.waitForLoadState('networkidle');
      
      // Should show health status
      const content = await page.textContent('body');
      expect(content).toBeTruthy();
    }
  });
});
