# Setup GitHub Deployment for PoVicTranslate
# This script helps configure GitHub Actions to deploy to Azure App Service

Write-Host "=== GitHub Actions Deployment Setup ===" -ForegroundColor Cyan
Write-Host ""

$resourceGroup = "PoVicTranslate"
$appName = "PoVicTranslate"
$repoOwner = "punkouter26"
$repoName = "PoVicTranslate"

Write-Host "Step 1: Getting Azure App Service Publish Profile..." -ForegroundColor Yellow

try {
    # Get the publish profile from Azure
    $publishProfile = az webapp deployment list-publishing-profiles --resource-group $resourceGroup --name $appName --xml
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Successfully retrieved publish profile" -ForegroundColor Green
        
        # Save to a temp file
        $publishProfile | Out-File -FilePath "publish-profile.xml" -Encoding UTF8
        
        Write-Host ""
        Write-Host "Step 2: Add this as a GitHub Secret" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "1. Go to: https://github.com/$repoOwner/$repoName/settings/secrets/actions" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "2. Click 'New repository secret'" -ForegroundColor White
        Write-Host ""
        Write-Host "3. Name: AZURE_WEBAPP_PUBLISH_PROFILE" -ForegroundColor White
        Write-Host ""
        Write-Host "4. Value: Copy the content from publish-profile.xml" -ForegroundColor White
        Write-Host ""
        Write-Host "   File saved at: $(Get-Location)\publish-profile.xml" -ForegroundColor Gray
        Write-Host ""
        Write-Host "5. Click 'Add secret'" -ForegroundColor White
        Write-Host ""
        Write-Host "Alternative: Use GitHub CLI to add the secret automatically:" -ForegroundColor Yellow
        Write-Host "   gh secret set AZURE_WEBAPP_PUBLISH_PROFILE < publish-profile.xml" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "=== Setup Instructions Complete ===" -ForegroundColor Green
        Write-Host ""
        Write-Host "After adding the secret, push your code to trigger the deployment:" -ForegroundColor Yellow
        Write-Host "   git push origin master" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Monitor deployment at:" -ForegroundColor Yellow
        Write-Host "   https://github.com/$repoOwner/$repoName/actions" -ForegroundColor Cyan
    }
    else {
        throw "Failed to get publish profile"
    }
}
catch {
    Write-Host ""
    Write-Host "=== Error Getting Publish Profile ===" -ForegroundColor Red
    Write-Host ""
    Write-Host "Manual method:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. Go to Azure Portal: https://portal.azure.com" -ForegroundColor White
    Write-Host ""
    Write-Host "2. Navigate to: App Services > $appName" -ForegroundColor White
    Write-Host ""
    Write-Host "3. Click 'Get publish profile' in the top menu" -ForegroundColor White
    Write-Host ""
    Write-Host "4. Open the downloaded .PublishSettings file in a text editor" -ForegroundColor White
    Write-Host ""
    Write-Host "5. Copy the entire XML content" -ForegroundColor White
    Write-Host ""
    Write-Host "6. Go to: https://github.com/$repoOwner/$repoName/settings/secrets/actions" -ForegroundColor White
    Write-Host ""
    Write-Host "7. Create a new secret named: AZURE_WEBAPP_PUBLISH_PROFILE" -ForegroundColor White
    Write-Host ""
    Write-Host "8. Paste the XML content as the value" -ForegroundColor White
    Write-Host ""
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
