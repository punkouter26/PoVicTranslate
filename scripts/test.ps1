# Test Script - Run all unit and integration tests
# Usage: ./scripts/test.ps1

Write-Host "Running all tests for Po.VicTranslate..." -ForegroundColor Cyan

# Run unit tests
Write-Host "`nRunning Unit Tests..." -ForegroundColor Yellow
dotnet test ./tests/Po.VicTranslate.UnitTests/Po.VicTranslate.UnitTests.csproj --configuration Release --no-build --verbosity normal

# Run integration tests
Write-Host "`nRunning Integration Tests..." -ForegroundColor Yellow
dotnet test ./tests/Po.VicTranslate.IntegrationTests/Po.VicTranslate.IntegrationTests.csproj --configuration Release --no-build --verbosity normal

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nAll tests passed!" -ForegroundColor Green
} else {
    Write-Host "`nTests failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}
