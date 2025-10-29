# Update Namespaces Script
# Updates all VictorianTranslator.* namespaces to Po.VicTranslate.*

Write-Host "Updating namespaces from VictorianTranslator.* to Po.VicTranslate.*..." -ForegroundColor Cyan

# Update API project
Get-ChildItem -Path ".\src\Po.VicTranslate.Api" -Recurse -Filter "*.cs" | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $updated = $content -replace 'namespace VictorianTranslator\.Server\.', 'namespace Po.VicTranslate.Api.'
    $updated = $updated -replace 'namespace VictorianTranslator\.', 'namespace Po.VicTranslate.Api.'
    $updated = $updated -replace 'using VictorianTranslator\.Server\.', 'using Po.VicTranslate.Api.'
    $updated = $updated -replace 'using VictorianTranslator\.', 'using Po.VicTranslate.Api.'
    Set-Content -Path $_.FullName -Value $updated -NoNewline
}

# Update Client project
Get-ChildItem -Path ".\src\Po.VicTranslate.Client" -Recurse -Filter "*.cs" | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $updated = $content -replace 'namespace VictorianTranslator\.Client\.', 'namespace Po.VicTranslate.Client.'
    $updated = $updated -replace 'namespace VictorianTranslator\.', 'namespace Po.VicTranslate.Client.'
    $updated = $updated -replace 'using VictorianTranslator\.Client\.', 'using Po.VicTranslate.Client.'
    $updated = $updated -replace 'using VictorianTranslator\.', 'using Po.VicTranslate.Client.'
    Set-Content -Path $_.FullName -Value $updated -NoNewline
}

# Update Razor files
Get-ChildItem -Path ".\src\Po.VicTranslate.Client" -Recurse -Filter "*.razor" | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $updated = $content -replace '@using VictorianTranslator\.Client\.', '@using Po.VicTranslate.Client.'
    $updated = $updated -replace '@using VictorianTranslator\.', '@using Po.VicTranslate.Client.'
    Set-Content -Path $_.FullName -Value $updated -NoNewline
}

# Update test projects
Get-ChildItem -Path ".\tests" -Recurse -Filter "*.cs" | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $updated = $content -replace 'using VictorianTranslator\.Server\.', 'using Po.VicTranslate.Api.'
    $updated = $updated -replace 'using VictorianTranslator\.', 'using Po.VicTranslate.Api.'
    Set-Content -Path $_.FullName -Value $updated -NoNewline
}

Write-Host "Namespace update completed!" -ForegroundColor Green
