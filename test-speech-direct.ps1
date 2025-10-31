# Direct test of Azure Speech Service credentials
$subscriptionKey = "41266d9a935649c48a4233a4861eb837"
$region = "eastus2"
$text = "Hello, testing."

Write-Host "Testing Azure Speech Service Directly" -ForegroundColor Cyan
Write-Host "Region: $region" -ForegroundColor Yellow
Write-Host "Key: $($subscriptionKey.Substring(0,8))..." -ForegroundColor Yellow
Write-Host ""

# Test using the Azure Speech REST API
$url = "https://$region.tts.speech.microsoft.com/cognitiveservices/v1"

$headers = @{
    "Ocp-Apim-Subscription-Key" = $subscriptionKey
    "Content-Type" = "application/ssml+xml"
    "X-Microsoft-OutputFormat" = "audio-16khz-32kbitrate-mono-mp3"
}

$ssml = @"
<speak version='1.0' xml:lang='en-GB'>
    <voice xml:lang='en-GB' name='en-GB-RyanNeural'>
        $text
    </voice>
</speak>
"@

try {
    Write-Host "Calling Azure Speech Service REST API..." -ForegroundColor Yellow
    $response = Invoke-WebRequest -Uri $url -Method Post -Headers $headers -Body $ssml -ErrorAction Stop
    
    Write-Host "✓ Success!" -ForegroundColor Green
    Write-Host "✓ Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "✓ Audio Size: $($response.Content.Length) bytes" -ForegroundColor Green
    
    # Save audio
    $outputFile = "direct-test.mp3"
    [System.IO.File]::WriteAllBytes($outputFile, $response.Content)
    Write-Host "✓ Saved to: $outputFile" -ForegroundColor Green
    
} catch {
    Write-Host "✗ Failed!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "Status Code: $statusCode" -ForegroundColor Red
        
        try {
            $result = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($result)
            $responseBody = $reader.ReadToEnd()
            Write-Host "Response: $responseBody" -ForegroundColor Yellow
        } catch {
            Write-Host "Could not read response" -ForegroundColor Yellow
        }
    }
}
