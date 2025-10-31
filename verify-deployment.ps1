# Deployment Verification Script
# This script verifies the Azure Speech Service deployment and configuration

param(
    [string]$AppUrl = "https://povictranslate.azurewebsites.net"
)

Write-Host "=== PoVicTranslate Deployment Verification ===" -ForegroundColor Cyan
Write-Host ""

# Function to test endpoint
function Test-Endpoint {
    param(
        [string]$Url,
        [string]$Description
    )
    
    Write-Host "Testing: $Description" -ForegroundColor Yellow
    Write-Host "URL: $Url" -ForegroundColor Gray
    
    try {
        $response = Invoke-WebRequest -Uri $Url -Method GET -TimeoutSec 30 -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Write-Host "✓ SUCCESS - Status: $($response.StatusCode)" -ForegroundColor Green
            
            # Try to parse as JSON if possible
            try {
                $json = $response.Content | ConvertFrom-Json
                Write-Host "Response:" -ForegroundColor Gray
                $json | ConvertTo-Json -Depth 3 | Write-Host -ForegroundColor White
            } catch {
                Write-Host "Response (first 200 chars): $($response.Content.Substring(0, [Math]::Min(200, $response.Content.Length)))" -ForegroundColor White
            }
            return $true
        } else {
            Write-Host "✗ FAILED - Status: $($response.StatusCode)" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "✗ ERROR: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
    Write-Host ""
}

# Function to test POST endpoint
function Test-PostEndpoint {
    param(
        [string]$Url,
        [string]$Description,
        [string]$Body
    )
    
    Write-Host "Testing: $Description" -ForegroundColor Yellow
    Write-Host "URL: $Url" -ForegroundColor Gray
    Write-Host "Body: $Body" -ForegroundColor Gray
    
    try {
        $jsonBody = $Body | ConvertTo-Json
        $response = Invoke-WebRequest -Uri $Url -Method POST -Body $jsonBody -ContentType "application/json" -TimeoutSec 30 -UseBasicParsing
        
        if ($response.StatusCode -eq 200) {
            Write-Host "✓ SUCCESS - Status: $($response.StatusCode)" -ForegroundColor Green
            Write-Host "Content-Type: $($response.Headers.'Content-Type')" -ForegroundColor Gray
            Write-Host "Content-Length: $($response.Content.Length) bytes" -ForegroundColor Gray
            return $true
        } else {
            Write-Host "✗ FAILED - Status: $($response.StatusCode)" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "✗ ERROR: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "Response: $responseBody" -ForegroundColor Red
        }
        return $false
    }
    Write-Host ""
}

# Wait for deployment to complete
Write-Host "Waiting for deployment to complete (30 seconds)..." -ForegroundColor Cyan
Start-Sleep -Seconds 30

# Test 1: Health endpoint
$healthUrl = "$AppUrl/health"
$healthResult = Test-Endpoint -Url $healthUrl -Description "Health Check Endpoint"

# Test 2: Speech configuration endpoint (NEW)
$configUrl = "$AppUrl/Speech/test-config"
$configResult = Test-Endpoint -Url $configUrl -Description "Speech Configuration Test (NEW)"

# Test 3: Speech synthesis endpoint
Write-Host "Testing: Speech Synthesis Endpoint" -ForegroundColor Yellow
Write-Host "URL: $AppUrl/Speech/synthesize" -ForegroundColor Gray
Write-Host "This will test the actual Azure Speech Service integration..." -ForegroundColor Gray
$synthesisResult = Test-PostEndpoint -Url "$AppUrl/Speech/synthesize" -Description "Speech Synthesis" -Body "Hello, good day to you"

# Summary
Write-Host ""
Write-Host "=== Deployment Verification Summary ===" -ForegroundColor Cyan
Write-Host "Health Check: $(if ($healthResult) { '✓ PASSED' } else { '✗ FAILED' })" -ForegroundColor $(if ($healthResult) { 'Green' } else { 'Red' })
Write-Host "Speech Config Test: $(if ($configResult) { '✓ PASSED' } else { '✗ FAILED' })" -ForegroundColor $(if ($configResult) { 'Green' } else { 'Red' })
Write-Host "Speech Synthesis: $(if ($synthesisResult) { '✓ PASSED' } else { '✗ FAILED' })" -ForegroundColor $(if ($synthesisResult) { 'Green' } else { 'Red' })

Write-Host ""
if ($healthResult -and $configResult) {
    Write-Host "✓ Deployment verification SUCCESSFUL!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Cyan
    Write-Host "1. Open the app: $AppUrl" -ForegroundColor White
    Write-Host "2. Test the translation feature" -ForegroundColor White
    Write-Host "3. Click 'HEAR IT SPOKEN' to test speech synthesis" -ForegroundColor White
    
    if (-not $synthesisResult) {
        Write-Host ""
        Write-Host "⚠ Speech Synthesis failed - You may need to:" -ForegroundColor Yellow
        Write-Host "  1. Run .\regenerate-speech-keys.ps1 to get fresh Azure Speech keys" -ForegroundColor White
        Write-Host "  2. Update the Azure App Service configuration with the new keys" -ForegroundColor White
        Write-Host "  3. Restart the App Service" -ForegroundColor White
    }
} else {
    Write-Host "✗ Deployment verification FAILED!" -ForegroundColor Red
    Write-Host "Check the GitHub Actions logs for more details." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "GitHub Actions: https://github.com/punkouter26/PoVicTranslate/actions" -ForegroundColor Gray
Write-Host "Azure Portal: https://portal.azure.com" -ForegroundColor Gray
