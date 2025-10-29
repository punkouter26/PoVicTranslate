import { test, expect } from '@playwright/test';

test.describe('Azure Deployed App - Complete Translation Flow', () => {
  test('should translate text and play audio', async ({ page }) => {
    // Navigate to the deployed Azure app
    await page.goto('https://povictranslate.azurewebsites.net/', { 
      waitUntil: 'domcontentloaded',
      timeout: 60000 
    });

    console.log('Page loaded, waiting for Blazor to initialize...');
    
    // Wait for Blazor app to fully load
    await page.waitForLoadState('networkidle', { timeout: 60000 });
    
    // Look for the input textarea
    console.log('Looking for input field...');
    const inputField = page.locator('textarea').first();
    await inputField.waitFor({ state: 'visible', timeout: 30000 });
    
    // Enter some simple text
    const testText = 'Hello, how are you today?';
    console.log(`Entering text: "${testText}"`);
    await inputField.fill(testText);
    
    // Find and click the translate button
    console.log('Looking for translate button...');
    const translateButton = page.locator('button:has-text("Transform to Victorian")').first();
    await translateButton.waitFor({ state: 'visible', timeout: 10000 });
    await translateButton.click();
    
    console.log('Clicked translate button, waiting for translation...');
    
    // Wait for translation result to appear (could take a few seconds for API call)
    await page.waitForTimeout(5000);
    
    // Check if there's an error message
    const errorMessage = page.locator('text=/error|Error|500|failed/i');
    const hasError = await errorMessage.count() > 0;
    
    if (hasError) {
      const errorText = await errorMessage.first().textContent();
      console.log(`ERROR FOUND: ${errorText}`);
      
      // Take a screenshot of the error
      await page.screenshot({ 
        path: 'azure-translation-error.png',
        fullPage: true 
      });
      
      // Fail the test but with detailed info
      throw new Error(`Translation failed with error: ${errorText}`);
    }
    
    // Look for the "Hear it Spoken" button (speech synthesis)
    console.log('Looking for speech button...');
    const speechButton = page.locator('button:has-text("HEAR IT SPOKEN"), button:has-text("Hear it Spoken")');
    const hasSpeechButton = await speechButton.count() > 0;
    
    if (!hasSpeechButton) {
      console.log('Speech button not found, taking screenshot...');
      await page.screenshot({ 
        path: 'azure-no-speech-button.png',
        fullPage: true 
      });
      throw new Error('Speech button not found after translation');
    }
    
    // Click the speech button
    await speechButton.first().click();
    console.log('Clicked speech button, waiting for audio...');
    
    // Wait a moment for the audio request
    await page.waitForTimeout(3000);
    
    // Check for speech errors
    const speechError = page.locator('text=/Text-to-speech error|speech service|An unexpected error/i');
    const hasSpeechError = await speechError.count() > 0;
    
    if (hasSpeechError) {
      const speechErrorText = await speechError.first().textContent();
      console.log(`SPEECH ERROR FOUND: ${speechErrorText}`);
      
      await page.screenshot({ 
        path: 'azure-speech-error.png',
        fullPage: true 
      });
      
      throw new Error(`Speech synthesis failed: ${speechErrorText}`);
    }
    
    console.log('✓ Complete flow successful: Text translated and audio played');
    
    // Take a success screenshot
    await page.screenshot({ 
      path: 'azure-translation-success.png',
      fullPage: true 
    });
  });
});
