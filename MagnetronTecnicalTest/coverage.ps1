# Script rápido para generar y ver cobertura de código
# coverage.ps1

Write-Host "?? Generando reporte de cobertura de código..." -ForegroundColor Green

# Limpiar resultados anteriores
if (Test-Path "TestResults") {
    Remove-Item -Path "TestResults" -Recurse -Force
    Write-Host "?? Limpiando resultados anteriores..." -ForegroundColor Blue
}

# Ejecutar pruebas con cobertura
Write-Host "?? Ejecutando pruebas con cobertura..." -ForegroundColor Blue
dotnet test MagnetronTecnicalTest.Tests\MagnetronTecnicalTest.Tests.csproj `
    --collect:"XPlat Code Coverage" `
    --results-directory:"TestResults" `
    --verbosity:minimal `
    --configuration:Release `
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover,cobertura `
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude="[*.Tests]*,[*.TestResults]*"

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error ejecutando pruebas" -ForegroundColor Red
    exit 1
}

# Verificar instalación de ReportGenerator
Write-Host "?? Verificando ReportGenerator..." -ForegroundColor Blue
$reportGenerator = & dotnet tool list -g reportgenerator 2>$null
if (-not $reportGenerator -or $reportGenerator -notmatch "reportgenerator") {
    Write-Host "?? Instalando ReportGenerator..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-reportgenerator-globaltool
    if ($LASTEXITCODE -ne 0) {
        Write-Host "? Error instalando ReportGenerator" -ForegroundColor Red
        exit 1
    }
}

# Buscar archivo de cobertura más reciente
$coverageFiles = Get-ChildItem -Path "TestResults" -Filter "coverage.cobertura.xml" -Recurse
if ($coverageFiles.Count -eq 0) {
    $coverageFiles = Get-ChildItem -Path "TestResults" -Filter "*.cobertura.xml" -Recurse
}

if ($coverageFiles.Count -eq 0) {
    Write-Host "? No se encontraron archivos de cobertura" -ForegroundColor Red
    Write-Host "?? Archivos encontrados en TestResults:" -ForegroundColor Yellow
    Get-ChildItem -Path "TestResults" -Recurse | ForEach-Object { Write-Host "   $($_.FullName)" -ForegroundColor Gray }
    exit 1
}

$latestCoverage = $coverageFiles | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Write-Host "?? Usando archivo de cobertura: $($latestCoverage.Name)" -ForegroundColor Cyan

# Generar reporte HTML
$reportPath = "CoverageReport"
if (Test-Path $reportPath) {
    Remove-Item -Path $reportPath -Recurse -Force
}

Write-Host "?? Generando reporte HTML..." -ForegroundColor Blue
reportgenerator `
    -reports:"$($latestCoverage.FullName)" `
    -targetdir:"$reportPath" `
    -reporttypes:"Html;HtmlSummary;Badges;TextSummary;MarkdownSummaryGithub"

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error generando reporte" -ForegroundColor Red
    exit 1
}

# Mostrar resumen
$summaryFile = Join-Path $reportPath "Summary.txt"
if (Test-Path $summaryFile) {
    Write-Host "`n?? === RESUMEN DE COBERTURA ===" -ForegroundColor Green
    Get-Content $summaryFile | Write-Host
    Write-Host ""
}

# Información sobre los archivos generados
Write-Host "?? Reporte generado exitosamente:" -ForegroundColor Green
Write-Host "   ?? Directorio: $reportPath" -ForegroundColor White
Write-Host "   ?? Reporte HTML: $reportPath\index.html" -ForegroundColor White
Write-Host "   ?? Resumen: $reportPath\Summary.txt" -ForegroundColor White
Write-Host "   ???  Badges: $reportPath\badge_*.svg" -ForegroundColor White

# Preguntar si abrir el reporte
$response = Read-Host "`n¿Abrir el reporte en el navegador? (s/n)"
if ($response -eq "s" -or $response -eq "S" -or $response -eq "si" -or $response -eq "Si") {
    $indexPath = Join-Path $reportPath "index.html"
    if (Test-Path $indexPath) {
        Write-Host "?? Abriendo reporte..." -ForegroundColor Blue
        Start-Process $indexPath
    }
}

Write-Host "`n? ¡Reporte de cobertura completado!" -ForegroundColor Green