# Phase 5: Deployment & CI/CD Setup Guide

## Overview

This guide configures GitHub Actions to build and deploy PoVicTranslate to Azure App Service using federated credentials (OIDC) for secure, passwordless authentication.

## Prerequisites

- Azure subscription with permissions to create App Registrations
- GitHub repository with admin access
- Azure CLI installed locally
- Existing F1 App Service Plan: `PoShared3` in resource group `PoShared`

## Step 1: Create Azure App Registration

### Using Azure CLI

```bash
# Set variables
GITHUB_REPO="punkouter26/PoVicTranslate"
APP_NAME="PoVicTranslate-GitHub-Deploy"
SUBSCRIPTION_ID="<your-subscription-id>"

# Create App Registration
az ad app create --display-name $APP_NAME

# Get the Application (client) ID
APP_ID=$(az ad app list --display-name $APP_NAME --query "[0].appId" -o tsv)
echo "Application (Client) ID: $APP_ID"

# Create Service Principal
az ad sp create --id $APP_ID

# Get Service Principal Object ID
SP_OBJECT_ID=$(az ad sp list --display-name $APP_NAME --query "[0].id" -o tsv)
echo "Service Principal Object ID: $SP_OBJECT_ID"
```

### Using Azure Portal

1. Navigate to **Azure Active Directory** > **App registrations**
2. Click **New registration**
3. Name: `PoVicTranslate-GitHub-Deploy`
4. Click **Register**
5. Copy the **Application (client) ID**
6. Copy the **Directory (tenant) ID**

## Step 2: Configure Federated Credentials

### Using Azure CLI

```bash
# Configure federated credential for main branch
az ad app federated-credential create \
  --id $APP_ID \
  --parameters '{
    "name": "PoVicTranslate-Main-Branch",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:'$GITHUB_REPO':ref:refs/heads/master",
    "description": "GitHub Actions deployment for master branch",
    "audiences": ["api://AzureADTokenExchange"]
  }'

# Configure federated credential for workflow dispatch
az ad app federated-credential create \
  --id $APP_ID \
  --parameters '{
    "name": "PoVicTranslate-Workflow-Dispatch",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:'$GITHUB_REPO':environment:production",
    "description": "GitHub Actions manual deployment",
    "audiences": ["api://AzureADTokenExchange"]
  }'
```

### Using Azure Portal

1. In App Registration, go to **Certificates & secrets**
2. Select **Federated credentials** tab
3. Click **Add credential**
4. Select **GitHub Actions deploying Azure resources**
5. Fill in:
   - **Organization**: `punkouter26`
   - **Repository**: `PoVicTranslate`
   - **Entity type**: `Branch`
   - **GitHub branch name**: `master`
   - **Name**: `PoVicTranslate-Main-Branch`
6. Click **Add**

## Step 3: Assign Azure Permissions

### Using Azure CLI

```bash
# Assign Contributor role at subscription level (or resource group level)
az role assignment create \
  --assignee $APP_ID \
  --role Contributor \
  --scope /subscriptions/$SUBSCRIPTION_ID

# Or assign at Resource Group level only
RESOURCE_GROUP="PoVicTranslate"
az role assignment create \
  --assignee $APP_ID \
  --role Contributor \
  --scope /subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP
```

### Using Azure Portal

1. Navigate to **Subscriptions** > Select your subscription
2. Click **Access control (IAM)**
3. Click **Add** > **Add role assignment**
4. Select **Contributor** role
5. Click **Next**
6. Click **Select members**
7. Search for `PoVicTranslate-GitHub-Deploy`
8. Select it and click **Select**
9. Click **Review + assign**

## Step 4: Configure GitHub Secrets

### Required Secrets

Navigate to GitHub repository: `Settings` > `Secrets and variables` > `Actions`

Click **New repository secret** for each:

| Secret Name | Value | Where to Find |
|:---|:---|:---|
| `AZURE_CLIENT_ID` | Application (client) ID | App Registration > Overview |
| `AZURE_TENANT_ID` | Directory (tenant) ID | App Registration > Overview |
| `AZURE_SUBSCRIPTION_ID` | Your subscription ID | Azure Portal > Subscriptions |

### Using GitHub CLI

```bash
# Set secrets using GitHub CLI
gh secret set AZURE_CLIENT_ID --body "$APP_ID"
gh secret set AZURE_TENANT_ID --body "$(az account show --query tenantId -o tsv)"
gh secret set AZURE_SUBSCRIPTION_ID --body "$SUBSCRIPTION_ID"
```

## Step 5: Deploy Azure Infrastructure

### Using Azure Developer CLI (azd)

```bash
# Initialize azd (if not already done)
azd init

# Set environment name
azd env new PoVicTranslate

# Provision Azure resources (uses Bicep templates)
azd provision

# Or deploy both infrastructure and code
azd up
```

### Manual Deployment (Azure CLI)

```bash
# Deploy Bicep template
az deployment sub create \
  --location eastus2 \
  --template-file ./infra/main.bicep \
  --parameters ./infra/main.parameters.json

# Get App Service name
WEBAPP_NAME=$(az webapp list --resource-group PoVicTranslate --query "[0].name" -o tsv)
echo "Web App: $WEBAPP_NAME"
```

## Step 6: Configure App Service Settings

### Production API Keys

App Service settings are configured separately from appsettings.json for production.

```bash
# Set Application Insights connection string
az webapp config appsettings set \
  --name PoVicTranslate \
  --resource-group PoVicTranslate \
  --settings ApplicationInsights__ConnectionString="<your-connection-string>"

# Set Azure OpenAI settings
az webapp config appsettings set \
  --name PoVicTranslate \
  --resource-group PoVicTranslate \
  --settings \
    ApiSettings__AzureOpenAIApiKey="<your-key>" \
    ApiSettings__AzureOpenAIEndpoint="<your-endpoint>" \
    ApiSettings__AzureOpenAIDeploymentName="<your-deployment>"

# Set Azure Speech settings
az webapp config appsettings set \
  --name PoVicTranslate \
  --resource-group PoVicTranslate \
  --settings \
    ApiSettings__AzureSpeechSubscriptionKey="<your-key>" \
    ApiSettings__AzureSpeechRegion="eastus2"
```

### Using Azure Portal

1. Navigate to **App Service** > `PoVicTranslate`
2. Click **Settings** > **Environment variables**
3. Click **+ Add** for each setting
4. Enter **Name** and **Value**
5. Click **Apply** then **Confirm**

## Step 7: Test GitHub Actions Workflow

### Trigger Deployment

```bash
# Make a small change and push
git add .
git commit -m "Test: Trigger GitHub Actions deployment"
git push origin master
```

### Monitor Workflow

1. Navigate to GitHub repository
2. Click **Actions** tab
3. Click on the latest workflow run
4. Monitor build and deployment steps
5. Check for errors

### Verify Deployment

```bash
# Test health endpoint
curl https://povictranslate.azurewebsites.net/api/health

# Expected response:
# {"Status":"Healthy","Timestamp":"...","Checks":[...]}

# Test Swagger UI
# Open: https://povictranslate.azurewebsites.net/swagger
```

## Workflow Overview

### `.github/workflows/deploy.yml`

**Trigger:** Push to `master` branch or manual dispatch

**Steps:**
1. ✅ Checkout code
2. ✅ Setup .NET 9.0
3. ✅ Restore NuGet dependencies
4. ✅ Build solution (Release configuration)
5. ✅ Publish API project
6. ✅ Azure login (federated credentials)
7. ✅ Deploy to App Service
8. ✅ Verify deployment (health check)
9. ✅ Azure logout

**Duration:** ~3-5 minutes

## Deployment Architecture

```
┌──────────────────┐
│  GitHub Actions  │
│   (Build + CI)   │
└────────┬─────────┘
         │ OIDC Auth (Federated Credentials)
         ↓
┌──────────────────┐
│ Azure App Service│
│  PoVicTranslate  │ ← Uses PoShared3 F1 Plan
└────────┬─────────┘
         │
         ↓
┌──────────────────┐
│  Dependencies:   │
│ - App Insights   │
│ - Storage Acct   │
│ - Log Analytics  │
└──────────────────┘
```

## Local vs Production Configuration

### Local Development (Azurite)

- Uses `appsettings.Development.json`
- Azurite for Table Storage
- Local ports: HTTP 5000, HTTPS 5001
- Debug logging to console and file

### Production (Azure)

- Uses App Service environment variables
- Azure Table Storage
- HTTPS only (App Service domain)
- Application Insights telemetry
- Serilog to Application Insights

## Troubleshooting

### Federated Credential Issues

**Error:** "AADSTS70021: No matching federated identity record found"

**Solution:**
- Verify subject format: `repo:punkouter26/PoVicTranslate:ref:refs/heads/master`
- Check issuer: `https://token.actions.githubusercontent.com`
- Ensure audience: `api://AzureADTokenExchange`

### Permission Issues

**Error:** "Authorization failed"

**Solution:**
- Verify Service Principal has Contributor role
- Check role assignment scope (subscription or resource group)
- Wait 5-10 minutes for permissions to propagate

### Deployment Failures

**Error:** Build or deployment step fails

**Solution:**
- Check GitHub Actions logs for specific error
- Verify App Service name matches in workflow
- Ensure .NET version matches (9.0)
- Check App Service logs in Azure Portal

### Health Check Fails

**Error:** `/api/health` returns 500 or timeout

**Solution:**
- Check App Service logs for startup errors
- Verify Application Settings are configured
- Check Azure OpenAI and Speech service keys
- Review Application Insights for exceptions

## Security Best Practices

✅ **Implemented:**
- Federated credentials (no passwords/secrets in GitHub)
- Secrets stored in GitHub repository secrets
- HTTPS-only App Service
- TLS 1.2 minimum
- Problem Details for error responses (no stack traces)

⚠️ **Note for Private Repo:**
- Sensitive data in `appsettings.json` is acceptable for this private repository
- Production values override via App Service settings
- Never commit production API keys to repository

## Monitoring Deployment

### GitHub Actions

- View workflow runs: Repository > Actions
- Check deployment logs for errors
- Monitor build duration and success rate

### Azure App Service

- **Logs:** App Service > Log stream
- **Metrics:** App Service > Metrics (CPU, Memory, Response Time)
- **Alerts:** Create alerts for deployment failures

### Application Insights

- **Live Metrics:** Real-time telemetry
- **Failures:** Exception tracking
- **Performance:** Request duration and dependencies
- **Custom Telemetry:** Business metrics (see docs/KQL/)

## Cost Optimization

- ✅ Uses existing **F1 Free tier** App Service Plan (`PoShared3`)
- ✅ Shared across multiple apps to minimize cost
- ✅ No additional App Service Plan charges
- 💡 Monitor Azure costs in Cost Management

## Next Steps

1. ✅ Complete this setup guide
2. ✅ Push code to trigger first deployment
3. ✅ Verify deployment success
4. ✅ Configure production API keys
5. ✅ Set up Azure Monitor alerts
6. ✅ Create Application Insights dashboard (use KQL queries from Phase 4)

---

**Phase 5 Complete:** CI/CD pipeline operational with GitHub Actions and Azure App Service deployment! 🚀
