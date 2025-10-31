# Fixing Azure Speech Service Error

## Problem
The application is showing an error: "Text-to-speech service unavailable: Speech service returned status InternalServerError"

## Most Likely Causes
1. **Expired or Invalid Azure Speech Service Key**
2. **Incorrect Region Configuration**
3. **Service Quota Exceeded**

## Quick Fix Steps

### Option 1: Regenerate Azure Speech Service Keys (Recommended)

1. **Run the regenerate script:**
   ```powershell
   .\regenerate-speech-keys.ps1
   ```

2. **If you get an error, manually retrieve the keys:**
   ```powershell
   # Login to Azure
   az login
   
   # List your resource groups
   az group list --query "[].name" -o table
   
   # List Cognitive Services in your resource group
   az cognitiveservices account list -g rg-povictranslate --query "[].{Name:name, Kind:kind, Location:location}" -o table
   
   # Get the keys
   az cognitiveservices account keys list --name <your-speech-service-name> --resource-group <your-rg-name>
   ```

3. **Update appsettings.json:**
   
   Open `src/Po.VicTranslate.Api/appsettings.json` and update:
   ```json
   {
     "ApiSettings": {
       "AzureSpeechSubscriptionKey": "YOUR_NEW_KEY_HERE",
       "AzureSpeechRegion": "eastus2"
     }
   }
   ```

### Option 2: Verify Configuration Via Azure Portal

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to your Speech Service resource
3. Click on "Keys and Endpoint" in the left menu
4. Copy **Key 1** or **Key 2**
5. Note the **Region**
6. Update your `appsettings.json` file with the correct key and region

### Option 3: Create a New Speech Service Resource

If you don't have a Speech Service resource:

```powershell
# Login to Azure
az login

# Create resource group (if needed)
az group create --name rg-povictranslate --location eastus2

# Create Speech Service
az cognitiveservices account create `
    --name povictranslate-speech `
    --resource-group rg-povictranslate `
    --kind SpeechServices `
    --sku F0 `
    --location eastus2 `
    --yes

# Get the keys
az cognitiveservices account keys list `
    --name povictranslate-speech `
    --resource-group rg-povictranslate
```

## Code Changes Made

The following improvements have been made to help diagnose and fix the issue:

1. **Enhanced Error Logging** - Added detailed logging to show which voice and region are being used
2. **Better Error Messages** - Error messages now include specific error codes (Authentication, BadRequest, etc.)
3. **SSML Support** - Using SSML for more reliable speech synthesis
4. **Voice Change** - Changed from `en-GB-LibbyNeural` to `en-GB-RyanNeural` (more widely available)
5. **Direct Audio Data** - Using `result.AudioData` directly for better reliability
6. **Test Endpoint** - Added `/Speech/test-config` endpoint to verify configuration

## Testing the Fix

After updating the configuration:

1. **Restart the application**
2. **Test the configuration endpoint:**
   ```
   GET https://povictranslate.azurewebsites.net/Speech/test-config
   ```
3. **Try the speech synthesis again**

## Common Error Codes

- **AuthenticationFailure** - Invalid subscription key
- **BadRequest** - Voice or region not supported
- **TooManyRequests** - Quota exceeded
- **ConnectionFailure** - Network issues
- **ServiceTimeout** - Azure service is slow/unavailable

## Need More Help?

Check the server logs for detailed error messages:
- Look for lines containing "Speech synthesis"
- Check the ErrorCode and ErrorDetails in the logs
