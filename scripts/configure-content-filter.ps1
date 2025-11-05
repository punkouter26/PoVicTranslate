# Configure Azure OpenAI Content Filter to allow low-severity content
# This sets the filter to block only Medium and High severity content

param(
    [string]$ResourceGroup = "PoVicTranslate",
    [string]$AccountName = "PoVicTranslate-OpenAI",
    [string]$DeploymentName = "gpt-4o"
)

Write-Host "Configuring content filter for Azure OpenAI..." -ForegroundColor Cyan
Write-Host "Resource Group: $ResourceGroup" -ForegroundColor Yellow
Write-Host "Account Name: $AccountName" -ForegroundColor Yellow
Write-Host "Deployment: $DeploymentName" -ForegroundColor Yellow
Write-Host ""

# Get account details
Write-Host "Getting Azure OpenAI account details..." -ForegroundColor Cyan
$account = az cognitiveservices account show `
    --name $AccountName `
    --resource-group $ResourceGroup `
    --query "{endpoint:properties.endpoint, location:location}" `
    -o json | ConvertFrom-Json

if (-not $account) {
    Write-Error "Failed to find Azure OpenAI account"
    exit 1
}

$endpoint = $account.endpoint.TrimEnd('/')
$location = $account.location

Write-Host "Endpoint: $endpoint" -ForegroundColor Green
Write-Host "Location: $location" -ForegroundColor Green
Write-Host ""

# Get access token
Write-Host "Getting access token..." -ForegroundColor Cyan
$token = az account get-access-token --resource https://cognitiveservices.azure.com --query accessToken -o tsv

if (-not $token) {
    Write-Error "Failed to get access token"
    exit 1
}

# Create content filter configuration
Write-Host "Creating content filter configuration..." -ForegroundColor Cyan

$contentFilterConfig = @{
    name = "AllowLowSeverity"
    policies = @{
        completion = @{
            hate = @{
                allowedContentLevel = "medium"
                blocking = $true
                enabled = $true
            }
            sexual = @{
                allowedContentLevel = "medium"
                blocking = $true
                enabled = $true
            }
            selfHarm = @{
                allowedContentLevel = "medium"
                blocking = $true
                enabled = $true
            }
            violence = @{
                allowedContentLevel = "medium"
                blocking = $true
                enabled = $true
            }
        }
        prompt = @{
            hate = @{
                allowedContentLevel = "medium"
                blocking = $true
                enabled = $true
            }
            sexual = @{
                allowedContentLevel = "medium"
                blocking = $true
                enabled = $true
            }
            selfHarm = @{
                allowedContentLevel = "medium"
                blocking = $true
                enabled = $true
            }
            violence = @{
                allowedContentLevel = "medium"
                blocking = $true
                enabled = $true
            }
        }
    }
} | ConvertTo-Json -Depth 10

# API endpoint for content filters (using Azure AI Studio API)
$apiVersion = "2024-10-01-preview"
$contentFilterUrl = "$endpoint/openai/contentfilters/AllowLowSeverity?api-version=$apiVersion"

Write-Host "Creating/Updating content filter 'AllowLowSeverity'..." -ForegroundColor Cyan
Write-Host "URL: $contentFilterUrl" -ForegroundColor Gray

try {
    $response = Invoke-RestMethod -Uri $contentFilterUrl `
        -Method Put `
        -Headers @{
            "Authorization" = "Bearer $token"
            "Content-Type" = "application/json"
        } `
        -Body $contentFilterConfig `
        -ErrorAction Stop

    Write-Host "✅ Content filter configuration created successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Configuration details:" -ForegroundColor Cyan
    $response | ConvertTo-Json -Depth 5
} catch {
    Write-Host "❌ Failed to create content filter configuration" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Note: Content filter configuration via REST API may require:" -ForegroundColor Yellow
    Write-Host "1. Specific API permissions" -ForegroundColor Yellow
    Write-Host "2. Configuration through Azure AI Studio portal instead" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Please use the Azure AI Studio portal to configure content filters:" -ForegroundColor Cyan
    Write-Host "1. Go to https://ai.azure.com/" -ForegroundColor White
    Write-Host "2. Select your resource: $AccountName" -ForegroundColor White
    Write-Host "3. Navigate to 'Content filters' section" -ForegroundColor White
    Write-Host "4. Create a new configuration with these settings:" -ForegroundColor White
    Write-Host "   - Name: AllowLowSeverity" -ForegroundColor White
    Write-Host "   - Severity: Medium, High (blocks medium and high, allows low)" -ForegroundColor White
    Write-Host "5. Associate it with the '$DeploymentName' deployment" -ForegroundColor White
    exit 1
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Go to Azure AI Studio: https://ai.azure.com/" -ForegroundColor White
Write-Host "2. Associate the 'AllowLowSeverity' filter with your deployment" -ForegroundColor White
Write-Host "3. Test the translation again" -ForegroundColor White
