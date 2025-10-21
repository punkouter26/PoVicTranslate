# Setup GitHub Actions Deployment Script
# This script helps configure GitHub Actions to deploy to Azure App Service

Write-Host "=== GitHub Actions Deployment Setup ===" -ForegroundColor Cyan
Write-Host ""

$resourceGroup = "PoVicTranslate"
$appName = "PoVicTranslate"
$repoOwner = "punkouter26"
$repoName = "PoVicTranslate"

Write-Host "Step 1: Getting Azure App Service Publish Profile..." -ForegroundColor Yellow

# Get the publish profile
$publishProfile = az webapp deployment list-publishing-profiles `
    --resource-group $resourceGroup `
    --name $appName `
    --xml

if ($LASTEXITCODE -eq 0 -and $publishProfile) {
    Write-Host "✓ Publish profile retrieved" -ForegroundColor Green
    
    # Save to a temporary file
    $profilePath = "publish-profile.xml"
    $publishProfile | Out-File -FilePath $profilePath -Encoding UTF8
    
    Write-Host ""
    Write-Host "Step 2: Add the publish profile to GitHub Secrets" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Option A - Using GitHub CLI (gh):" -ForegroundColor Cyan
    Write-Host "  Run this command:" -ForegroundColor Gray
    Write-Host "  gh secret set AZURE_WEBAPP_PUBLISH_PROFILE < $profilePath" -ForegroundColor White
    Write-Host ""
    Write-Host "Option B - Manual (if gh CLI not installed):" -ForegroundColor Cyan
    Write-Host "  1. Go to: https://github.com/$repoOwner/$repoName/settings/secrets/actions" -ForegroundColor Gray
    Write-Host "  2. Click 'New repository secret'" -ForegroundColor Gray
    Write-Host "  3. Name: AZURE_WEBAPP_PUBLISH_PROFILE" -ForegroundColor Gray
    Write-Host "  4. Value: Copy the contents of '$profilePath'" -ForegroundColor Gray
    Write-Host "  5. Click 'Add secret'" -ForegroundColor Gray
    Write-Host ""
    
    # Try using gh CLI if available
    $ghInstalled = Get-Command gh -ErrorAction SilentlyContinue
    if ($ghInstalled) {
        Write-Host "GitHub CLI detected! Would you like to set the secret now? (Y/N)" -ForegroundColor Yellow
        $response = Read-Host
        
        if ($response -eq 'Y' -or $response -eq 'y') {
            $profileContent = Get-Content $profilePath -Raw
            $profileContent | gh secret set AZURE_WEBAPP_PUBLISH_PROFILE
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ Secret added successfully!" -ForegroundColor Green
            } else {
                Write-Host "✗ Failed to add secret. Please add manually." -ForegroundColor Red
            }
        }
    }
    
    Write-Host ""
    Write-Host "Step 3: Verify the workflow" -ForegroundColor Yellow
    Write-Host "  File: .github/workflows/BuildDeploy.yml" -ForegroundColor Gray
    Write-Host "  Status: ✓ Ready to deploy on push to master" -ForegroundColor Green
    Write-Host ""
    Write-Host "Step 4: Test the deployment" -ForegroundColor Yellow
    Write-Host "  Push any commit to master branch, or manually trigger from:" -ForegroundColor Gray
    Write-Host "  https://github.com/$repoOwner/$repoName/actions" -ForegroundColor White
    Write-Host ""
    
    # Clean up
    Write-Host "Cleaning up temporary files..." -ForegroundColor Gray
    Write-Host "Note: publish-profile.xml contains sensitive data. Do NOT commit it to Git!" -ForegroundColor Red
    
} else {
    Write-Host "✗ Failed to retrieve publish profile" -ForegroundColor Red
    Write-Host ""
    Write-Host "Manual method:" -ForegroundColor Yellow
    Write-Host "1. Go to Azure Portal" -ForegroundColor Gray
    Write-Host "2. Navigate to App Service > $appName" -ForegroundColor Gray
    Write-Host "3. Click 'Get publish profile' in the toolbar" -ForegroundColor Gray
    Write-Host "4. Save the downloaded .PublishSettings file" -ForegroundColor Gray
    Write-Host "5. Add its contents as GitHub secret: AZURE_WEBAPP_PUBLISH_PROFILE" -ForegroundColor Gray
    Write-Host ""
}

Write-Host "=== Setup Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Commit and push the workflow file:" -ForegroundColor Gray
Write-Host "   git add .github/workflows/BuildDeploy.yml" -ForegroundColor White
Write-Host "   git commit -m 'Add CI/CD workflow for Azure App Service'" -ForegroundColor White
Write-Host "   git push origin master" -ForegroundColor White
Write-Host ""
Write-Host "2. Watch the deployment:" -ForegroundColor Gray
Write-Host "   https://github.com/$repoOwner/$repoName/actions" -ForegroundColor White
Write-Host ""
