$ErrorActionPreference = "Stop"

Write-Host "Running JSHint..." -ForegroundColor Green
& npm run lint
if ($LASTEXITCODE -ne 0) {
    throw "npm run lint failed with exit code $LASTEXITCODE"
}

Write-Host "Running mocha tests..." -ForegroundColor Green
& npm test
if ($LASTEXITCODE -ne 0) {
    throw "npm test failed with exit code $LASTEXITCODE"
}

Write-Host "Build successful." -ForegroundColor Green
