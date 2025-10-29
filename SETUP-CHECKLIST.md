# Phase 5: Quick Setup Checklist

## ⚠️ GitHub Actions will FAIL until these steps are completed

The workflow was triggered by the push but requires Azure configuration first.

## Required Setup Steps (In Order)

### 1. Get Your Azure Subscription ID
```bash
az login
az account show --query id -o tsv
# Copy this GUID
```

### 2. Create App Registration in Azure

**Option A - Azure CLI (Recommended):**
```bash
# Set variables (REPLACE with your values)
export GITHUB_REPO="punkouter26/PoVicTranslate"
export APP_NAME="PoVicTranslate-GitHub-Deploy"
export SUBSCRIPTION_ID="<paste-your-subscription-id-here>"

# Create App Registration
az ad app create --display-name "$APP_NAME"

# Get Application ID
export APP_ID=$(az ad app list --display-name "$APP_NAME" --query "[0].appId" -o tsv)
echo "✅ Application (Client) ID: $APP_ID"

# Create Service Principal
az ad sp create --id $APP_ID

# Add federated credential for master branch
az ad app federated-credential create \
  --id $APP_ID \
  --parameters "{
    \"name\": \"PoVicTranslate-Master\",
    \"issuer\": \"https://token.actions.githubusercontent.com\",
    \"subject\": \"repo:$GITHUB_REPO:ref:refs/heads/master\",
    \"description\": \"GitHub Actions for master branch\",
    \"audiences\": [\"api://AzureADTokenExchange\"]
  }"

# Assign Contributor role
az role assignment create \
  --assignee $APP_ID \
  --role Contributor \
  --scope /subscriptions/$SUBSCRIPTION_ID

# Get Tenant ID
export TENANT_ID=$(az account show --query tenantId -o tsv)
echo "✅ Tenant ID: $TENANT_ID"

# Summary - Copy these values for GitHub secrets:
echo ""
echo "═══════════════════════════════════════════"
echo "Copy these values to GitHub Secrets:"
echo "═══════════════════════════════════════════"
echo "AZURE_CLIENT_ID:       $APP_ID"
echo "AZURE_TENANT_ID:       $TENANT_ID"
echo "AZURE_SUBSCRIPTION_ID: $SUBSCRIPTION_ID"
echo "═══════════════════════════════════════════"
```

**Option B - Azure Portal:**
See `docs/DEPLOYMENT.md` for detailed Portal instructions.

### 3. Configure GitHub Secrets

Go to: https://github.com/punkouter26/PoVicTranslate/settings/secrets/actions

Click **New repository secret** three times:

| Name | Value |
|:---|:---|
| `AZURE_CLIENT_ID` | (from step 2) |
| `AZURE_TENANT_ID` | (from step 2) |
| `AZURE_SUBSCRIPTION_ID` | (from step 1) |

**Using GitHub CLI:**
```bash
gh secret set AZURE_CLIENT_ID --body "$APP_ID"
gh secret set AZURE_TENANT_ID --body "$TENANT_ID"
gh secret set AZURE_SUBSCRIPTION_ID --body "$SUBSCRIPTION_ID"
```

### 4. Deploy Azure Infrastructure

**Before GitHub Actions can deploy the app, the App Service must exist:**

```bash
# Deploy Bicep infrastructure
az deployment sub create \
  --location eastus2 \
  --template-file ./infra/main.bicep \
  --parameters ./infra/main.parameters.json

# Verify App Service was created
az webapp list --resource-group PoVicTranslate --query "[].name" -o tsv
# Should output: PoVicTranslate
```

### 5. Configure Production API Keys

```bash
# Get connection string from Application Insights
AI_CONN=$(az monitor app-insights component show \
  --app PoVicTranslate-insights \
  --resource-group PoVicTranslate \
  --query connectionString -o tsv)

# Set Application Insights
az webapp config appsettings set \
  --name PoVicTranslate \
  --resource-group PoVicTranslate \
  --settings "ApplicationInsights__ConnectionString=$AI_CONN"

# Set Azure OpenAI (REPLACE with your actual values)
az webapp config appsettings set \
  --name PoVicTranslate \
  --resource-group PoVicTranslate \
  --settings \
    "ApiSettings__AzureOpenAIApiKey=<your-openai-key>" \
    "ApiSettings__AzureOpenAIEndpoint=<your-openai-endpoint>" \
    "ApiSettings__AzureOpenAIDeploymentName=gpt-4o"

# Set Azure Speech (REPLACE with your actual values)
az webapp config appsettings set \
  --name PoVicTranslate \
  --resource-group PoVicTranslate \
  --settings \
    "ApiSettings__AzureSpeechSubscriptionKey=<your-speech-key>" \
    "ApiSettings__AzureSpeechRegion=eastus2"
```

### 6. Trigger Deployment

```bash
# Option 1: Push a change
echo "# Deployment test" >> README.md
git add README.md
git commit -m "Trigger GitHub Actions deployment"
git push origin master

# Option 2: Manual trigger in GitHub
# Go to Actions > Build and Deploy to Azure App Service > Run workflow
```

### 7. Monitor Deployment

**GitHub Actions:**
1. Go to: https://github.com/punkouter26/PoVicTranslate/actions
2. Click on the latest workflow run
3. Watch build and deploy steps
4. Check for errors (should see ✅ green checks)

**Expected Duration:** 3-5 minutes

### 8. Verify Deployment

```bash
# Test health endpoint
curl https://povictranslate.azurewebsites.net/api/health

# Expected response:
# {"Status":"Healthy","Timestamp":"2025-10-29T...","Checks":[...]}

# Open Swagger UI in browser:
# https://povictranslate.azurewebsites.net/swagger
```

## Troubleshooting

### Workflow Fails: "No matching federated identity"
- ✅ Check subject format: `repo:punkouter26/PoVicTranslate:ref:refs/heads/master`
- ✅ Verify issuer: `https://token.actions.githubusercontent.com`
- ✅ Wait 5 minutes after creating federated credential

### Workflow Fails: "Authorization failed"
- ✅ Verify Service Principal has Contributor role
- ✅ Check role scope includes subscription or resource group
- ✅ Wait 10 minutes for permissions to propagate

### Deployment Succeeds but Health Check Fails
- ✅ Check App Service logs in Azure Portal
- ✅ Verify Application Settings are configured
- ✅ Check API keys are valid (OpenAI, Speech)
- ✅ Review Application Insights for exceptions

### App Service Doesn't Exist
- ✅ Run `az deployment sub create` to deploy Bicep first
- ✅ Verify resource group "PoVicTranslate" exists
- ✅ Check deployment succeeded without errors

## Success Criteria ✅

When setup is complete, you should see:

1. ✅ GitHub Actions workflow completes with all green checks
2. ✅ App Service shows "Running" status in Azure Portal
3. ✅ Health endpoint returns `{"Status":"Healthy"}`
4. ✅ Swagger UI is accessible
5. ✅ No errors in App Service logs

## Next Steps After Successful Deployment

- Set up Azure Monitor alerts (docs/KQL/README.md)
- Create Application Insights dashboard
- Test translation API via Swagger
- Monitor custom telemetry in App Insights

---

**Need Help?** See full documentation in `docs/DEPLOYMENT.md`
