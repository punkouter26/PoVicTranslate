# PoVicTranslate Dashboard Deployment Script
param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroup,
    
    [Parameter(Mandatory=$true)]
    [string]$AppInsightsName,
    
    [Parameter(Mandatory=$false)]
    [string]$DashboardName = "PoVicTranslate-Monitoring-Dashboard",
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "eastus"
)

Write-Host "🚀 Deploying PoVicTranslate Monitoring Dashboard..." -ForegroundColor Cyan

# Verify Azure CLI is installed
if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Error "Azure CLI is not installed. Please install from: https://learn.microsoft.com/cli/azure/install-azure-cli"
    exit 1
}

# Check if logged in
$account = az account show 2>$null
if (-not $account) {
    Write-Host "Not logged in to Azure. Logging in..." -ForegroundColor Yellow
    az login
}

# Get Application Insights Resource ID
Write-Host "📊 Getting Application Insights resource ID..." -ForegroundColor Yellow
$appInsightsId = az monitor app-insights component show `
    --app $AppInsightsName `
    --resource-group $ResourceGroup `
    --query id `
    --output tsv

if (-not $appInsightsId) {
    Write-Error "Could not find Application Insights resource: $AppInsightsName in resource group: $ResourceGroup"
    exit 1
}

Write-Host "✅ Found Application Insights: $appInsightsId" -ForegroundColor Green

# Deploy the dashboard
Write-Host "🎨 Deploying dashboard template..." -ForegroundColor Yellow
az deployment group create `
    --resource-group $ResourceGroup `
    --template-file "infra/monitoring-dashboard.json" `
    --parameters `
        dashboardName=$DashboardName `
        applicationInsightsResourceId=$appInsightsId `
        location=$Location

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Dashboard deployed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "🌐 View your dashboard:" -ForegroundColor Cyan
    Write-Host "   1. Go to: https://portal.azure.com" -ForegroundColor White
    Write-Host "   2. Search for 'Dashboards'" -ForegroundColor White
    Write-Host "   3. Find '$DashboardName'" -ForegroundColor White
    Write-Host ""
    Write-Host "📊 Dashboard includes 12 monitoring tiles:" -ForegroundColor Cyan
    Write-Host "   • API Success Rate" -ForegroundColor White
    Write-Host "   • Response Time Statistics" -ForegroundColor White
    Write-Host "   • Cache Hit Rate" -ForegroundColor White
    Write-Host "   • Response Time Trends" -ForegroundColor White
    Write-Host "   • Request Volume" -ForegroundColor White
    Write-Host "   • Failed Endpoints" -ForegroundColor White
    Write-Host "   • Most Used Endpoints" -ForegroundColor White
    Write-Host "   • Cache Performance Over Time" -ForegroundColor White
    Write-Host "   • Azure Service Dependencies" -ForegroundColor White
    Write-Host "   • Exception Types" -ForegroundColor White
    Write-Host "   • Warning/Error Logs" -ForegroundColor White
    Write-Host "   • 24-Hour Health Summary" -ForegroundColor White
} else {
    Write-Error "Dashboard deployment failed. Check the error messages above."
    exit 1
}
