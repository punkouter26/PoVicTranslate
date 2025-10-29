# Run API locally
# Usage: ./scripts/run-api.ps1

Write-Host "Starting Po.VicTranslate API..." -ForegroundColor Cyan
Write-Host "API will be available at:" -ForegroundColor Yellow
Write-Host "  HTTP:  http://localhost:5000" -ForegroundColor Yellow
Write-Host "  HTTPS: https://localhost:5001" -ForegroundColor Yellow
Write-Host "  Swagger: https://localhost:5001/swagger" -ForegroundColor Yellow

Set-Location ./src/Po.VicTranslate.Api
dotnet run
