# Format Script - Apply dotnet format
# Usage: ./scripts/format.ps1

Write-Host "Formatting code with dotnet format..." -ForegroundColor Cyan

dotnet format --verify-no-changes

if ($LASTEXITCODE -eq 0) {
    Write-Host "Code is properly formatted!" -ForegroundColor Green
} else {
    Write-Host "Code formatting issues detected. Running formatter..." -ForegroundColor Yellow
    dotnet format
    Write-Host "Code has been formatted!" -ForegroundColor Green
}
