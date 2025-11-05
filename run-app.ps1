# Run the PoVicTranslate API
Write-Host "Starting PoVicTranslate API..."
Write-Host "Press Ctrl+C to stop"
Set-Location "src\Po.VicTranslate.Api"
dotnet run --launch-profile https
