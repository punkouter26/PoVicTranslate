# Build Script
# Usage: ./scripts/build.ps1

Write-Host "Building Po.VicTranslate solution..." -ForegroundColor Cyan

# Ensure we're using .NET 9.0.xxx (enforced by global.json)
dotnet --version

# Restore and build
dotnet restore
dotnet build --no-restore --configuration Release

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build succeeded!" -ForegroundColor Green
} else {
    Write-Host "Build failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}
