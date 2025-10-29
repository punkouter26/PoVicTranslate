# Start Azurite - Local Azure Storage Emulator
# Usage: ./scripts/start-azurite.ps1

Write-Host "Starting Azurite (Azure Storage Emulator)..." -ForegroundColor Cyan

# Check if Azurite is installed
if (-not (Get-Command azurite -ErrorAction SilentlyContinue)) {
    Write-Host "Azurite is not installed. Install with: npm install -g azurite" -ForegroundColor Red
    exit 1
}

# Start Azurite with custom workspace
azurite --location ./AzuriteConfig --debug ./azurite-debug.log

Write-Host "Azurite started successfully!" -ForegroundColor Green
