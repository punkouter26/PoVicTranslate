# Deploy to Azure App Service Script
# This script deploys the application to Azure using zip deploy

Write-Host "=== PoVicTranslate Azure Deployment ===" -ForegroundColor Cyan
Write-Host ""

# Configuration
$resourceGroup = "PoVicTranslate"
$appName = "PoVicTranslate"
$projectPath = "VictorianTranslator.Server"

Write-Host "1. Building Release version..." -ForegroundColor Yellow
Set-Location $PSScriptRoot
dotnet publish "$projectPath" -c Release -o "$projectPath/publish"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "2. Creating deployment package..." -ForegroundColor Yellow
$zipPath = "$projectPath/deploy.zip"
Remove-Item $zipPath -ErrorAction SilentlyContinue
Set-Location "$projectPath/publish"
Compress-Archive -Path * -DestinationPath "../deploy.zip" -Force
Set-Location $PSScriptRoot

Write-Host "3. Deploying to Azure..." -ForegroundColor Yellow
Write-Host "   App: $appName" -ForegroundColor Gray
Write-Host "   Package: $zipPath" -ForegroundColor Gray
Write-Host ""

# Option 1: Try using Azure CLI with retry
$maxRetries = 3
$retryCount = 0
$deployed = $false

while (-not $deployed -and $retryCount -lt $maxRetries) {
    try {
        Write-Host "   Deployment attempt $($retryCount + 1) of $maxRetries..." -ForegroundColor Gray
        az webapp deploy --resource-group $resourceGroup --name $appName --src-path $zipPath --type zip --timeout 600
        
        if ($LASTEXITCODE -eq 0) {
            $deployed = $true
            Write-Host ""
            Write-Host "=== Deployment Successful! ===" -ForegroundColor Green
            Write-Host "App URL: https://povictranslate.azurewebsites.net" -ForegroundColor Cyan
        }
    }
    catch {
        Write-Host "   Attempt failed: $_" -ForegroundColor Red
    }
    
    $retryCount++
    
    if (-not $deployed -and $retryCount -lt $maxRetries) {
        Write-Host "   Waiting 10 seconds before retry..." -ForegroundColor Yellow
        Start-Sleep -Seconds 10
    }
}

if (-not $deployed) {
    Write-Host ""
    Write-Host "=== Deployment Failed after $maxRetries attempts ===" -ForegroundColor Red
    Write-Host ""
    Write-Host "Alternative deployment options:" -ForegroundColor Yellow
    Write-Host "1. Azure Portal: Go to App Service > Deployment Center > Zip Deploy and upload:" -ForegroundColor Gray
    Write-Host "   $zipPath" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "2. Visual Studio Code: Right-click VictorianTranslator.Server > Deploy to Web App" -ForegroundColor Gray
    Write-Host ""
    exit 1
}

Write-Host ""
Write-Host "Checking deployment status..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Test the deployed app
try {
    $response = Invoke-WebRequest -Uri "https://povictranslate.azurewebsites.net/health" -TimeoutSec 30
    if ($response.StatusCode -eq 200) {
        Write-Host "Health check passed! ✓" -ForegroundColor Green
    }
}
catch {
    Write-Host "Health check pending (app may still be starting)..." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Deployment Complete ===" -ForegroundColor Green
