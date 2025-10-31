# Test Speech API with detailed error reporting
$baseUrl = "https://povictranslate.azurewebsites.net"
$testText = "I bid thee greetings, good Sir."

Write-Host "Testing Speech Synthesis API" -ForegroundColor Cyan
Write-Host "================================`n" -ForegroundColor Cyan

# Test 1: Configuration
Write-Host "1. Testing Configuration..." -ForegroundColor Yellow
try {
    $config = Invoke-RestMethod -Uri "$baseUrl/Speech/test-config" -Method Get
    Write-Host "   ✓ Configuration Valid: $($config.configurationValid)" -ForegroundColor Green
    Write-Host "   ✓ Has Key: $($config.hasSubscriptionKey)" -ForegroundColor Green
    Write-Host "   ✓ Region: $($config.region)" -ForegroundColor Green
    Write-Host "   ✓ Key Prefix: $($config.keyPrefix)" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Configuration test failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 2: Speech Synthesis
Write-Host "2. Testing Speech Synthesis..." -ForegroundColor Yellow
Write-Host "   Text: '$testText'" -ForegroundColor Gray

$headers = @{
    "Content-Type" = "application/json"
}

$body = ConvertTo-Json $testText

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/Speech/synthesize" -Method Post -Headers $headers -Body $body -ErrorAction Stop
    
    Write-Host "   ✓ Success!" -ForegroundColor Green
    Write-Host "   ✓ Status Code: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "   ✓ Content-Type: $($response.Headers['Content-Type'])" -ForegroundColor Green
    Write-Host "   ✓ Audio Size: $($response.RawContentLength) bytes" -ForegroundColor Green
    
    # Save the audio file
    $audioFile = "test-output.mp3"
    [System.IO.File]::WriteAllBytes($audioFile, $response.Content)
    Write-Host "   ✓ Saved audio to: $audioFile" -ForegroundColor Green
    
} catch {
    Write-Host "   ✗ Speech synthesis failed" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "   Status Code: $statusCode" -ForegroundColor Red
        
        try {
            $streamReader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
            $errorBody = $streamReader.ReadToEnd()
            $streamReader.Close()
            Write-Host "   Response Body: $errorBody" -ForegroundColor Yellow
        } catch {
            Write-Host "   Could not read error response body" -ForegroundColor Yellow
        }
    }
}

Write-Host "`n================================" -ForegroundColor Cyan
Write-Host "Test Complete" -ForegroundColor Cyan
