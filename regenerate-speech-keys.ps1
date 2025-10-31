# Script to regenerate Azure Speech Service keys
# This script helps you get the current keys from your Azure Speech Service resource

param(
    [string]$ResourceGroupName = "rg-povictranslate",
    [string]$SpeechServiceName = "povictranslate-speech"
)

Write-Host "Regenerating Azure Speech Service Keys..." -ForegroundColor Cyan
Write-Host ""

# Check if user is logged in to Azure
$account = az account show 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Please login to Azure first:" -ForegroundColor Yellow
    Write-Host "  az login" -ForegroundColor White
    exit 1
}

Write-Host "Current Azure account:" -ForegroundColor Green
az account show --query "{Name:name, SubscriptionId:id}" -o table

Write-Host ""
Write-Host "Fetching Speech Service keys..." -ForegroundColor Cyan

# List all cognitive services accounts to find the speech service
Write-Host "Looking for Speech Service resource: $SpeechServiceName in resource group: $ResourceGroupName" -ForegroundColor Yellow

# Get the keys
$keys = az cognitiveservices account keys list `
    --name $SpeechServiceName `
    --resource-group $ResourceGroupName `
    2>$null

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "Successfully retrieved keys!" -ForegroundColor Green
    Write-Host $keys | ConvertFrom-Json | ConvertTo-Json -Depth 10
    
    $keysObj = $keys | ConvertFrom-Json
    $key1 = $keysObj.key1
    
    Write-Host ""
    Write-Host "Update your appsettings.json with this key:" -ForegroundColor Cyan
    Write-Host "  AzureSpeechSubscriptionKey: $key1" -ForegroundColor White
    
    # Get the region
    $resourceDetails = az cognitiveservices account show `
        --name $SpeechServiceName `
        --resource-group $ResourceGroupName `
        --query "{location:location}" `
        -o json | ConvertFrom-Json
    
    Write-Host "  AzureSpeechRegion: $($resourceDetails.location)" -ForegroundColor White
    
} else {
    Write-Host ""
    Write-Host "Failed to retrieve keys. Please check:" -ForegroundColor Red
    Write-Host "  1. Resource group name: $ResourceGroupName" -ForegroundColor Yellow
    Write-Host "  2. Speech service name: $SpeechServiceName" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "List all resource groups:" -ForegroundColor Cyan
    az group list --query "[].name" -o table
    
    Write-Host ""
    Write-Host "List all Cognitive Services in a resource group:" -ForegroundColor Cyan
    Write-Host "  az cognitiveservices account list -g <resource-group-name> --query ""[].{Name:name, Kind:kind, Location:location}"" -o table" -ForegroundColor White
}
