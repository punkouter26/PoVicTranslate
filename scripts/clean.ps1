# Clean build artifacts and temporary files
# Usage: ./scripts/clean.ps1

Write-Host "Cleaning build artifacts..." -ForegroundColor Cyan

# Clean solution
dotnet clean

# Remove bin and obj folders
Get-ChildItem -Path . -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force

Write-Host "Clean completed!" -ForegroundColor Green
