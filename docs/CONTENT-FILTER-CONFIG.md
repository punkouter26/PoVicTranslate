# Azure OpenAI Content Filter Configuration Guide

## Problem
Azure OpenAI is blocking translations due to content filtering. We need to adjust the filters to allow low-severity content (like profanity in rap lyrics) while still blocking harmful content.

## Solution Options

### Option 1: Azure AI Studio Portal (EASIEST ⭐)

**Steps:**
1. Go to [Azure AI Studio](https://ai.azure.com/)
2. Sign in with your Azure credentials
3. Select your Azure OpenAI resource: **PoVicTranslate-OpenAI**
4. In the left menu, click **Content filters**
5. Click **+ Create content filter**
6. Configure:
   ```
   Name: AllowLowSeverity
   
   For PROMPTS:
   - Hate: Medium, High
   - Sexual: Medium, High  
   - Violence: Medium, High
   - Self-harm: Medium, High
   
   For COMPLETIONS:
   - Hate: Medium, High
   - Sexual: Medium, High
   - Violence: Medium, High
   - Self-harm: Medium, High
   ```
7. Click **Create**
8. Go to **Deployments** → Select **gpt-4o** deployment
9. Click **Edit**
10. Under **Content filter**, select **AllowLowSeverity**
11. Click **Save**

**Time required:** 2-3 minutes

---

### Option 2: PowerShell Script

Run the script created in `/scripts/configure-content-filter.ps1`:

```powershell
cd c:\Users\punko\Downloads\PoVicTranslate
.\scripts\configure-content-filter.ps1
```

**Note:** This may require additional permissions. If it fails, use Option 1.

---

### Option 3: Manual REST API (Advanced)

If you need to automate this in CI/CD:

```bash
# Get access token
TOKEN=$(az account get-access-token --resource https://cognitiveservices.azure.com --query accessToken -o tsv)

# Get endpoint
ENDPOINT=$(az cognitiveservices account show \
  --name PoVicTranslate-OpenAI \
  --resource-group PoVicTranslate \
  --query properties.endpoint -o tsv)

# Create content filter (requires specific API version and permissions)
# Note: This is complex and better done via portal
```

---

## What These Settings Mean

| Setting | What it blocks | What it allows |
|---------|---------------|----------------|
| **Low, Medium, High** (default) | Everything with any profanity/violence | Almost nothing explicit |
| **Medium, High** (recommended) | Moderate to severe content | Mild profanity, artistic content |
| **High only** | Only severe harmful content | Most artistic/creative content |

**Recommended for PoVicTranslate:** **Medium, High**
- Allows translation of rap lyrics with mild profanity
- Still blocks truly harmful content
- Balances creativity with responsibility

---

## Verification

After making changes:

1. Wait 1-2 minutes for changes to propagate
2. Refresh your app: https://localhost:5001
3. Select "Bastards" song
4. Click "Transform to Victorian"
5. Should now successfully translate!

---

## Troubleshooting

**"Changes not taking effect"**
- Wait 2-3 minutes for Azure to propagate changes
- Clear browser cache
- Restart the API if running

**"Can't find Content filters in portal"**
- Make sure you're in [Azure AI Studio](https://ai.azure.com/), not Azure Portal
- Ensure you have Contributor role on the resource

**"Still getting filtered"**
- Check that the content filter is associated with the deployment
- Try "High only" setting if "Medium, High" still blocks content
- Some content may be genuinely too extreme for any filter setting

---

## Current Resource Info

- **Resource Name:** PoVicTranslate-OpenAI
- **Resource Group:** PoVicTranslate
- **Location:** eastus2
- **Deployment:** gpt-4o
- **Current Filter:** Default (Low, Medium, High - blocks everything)
- **Target Filter:** AllowLowSeverity (Medium, High - allows mild content)
