# Script para ejecutar las pruebas unitarias del proyecto Magnetron
# run-tests.ps1

param(
    [string]$Configuration = "Release",
    [switch]$Watch = $false,
    [switch]$Coverage = $false,
    [switch]$OpenReport = $false,
    [string]$Filter = ""
)

Write-Host "=== Magnetron Technical Test - Test Runner ===" -ForegroundColor Green
Write-Host "Configuración: $Configuration" -ForegroundColor Yellow

# Verificar que el proyecto de pruebas existe
$testProject = "MagnetronTecnicalTest.Tests\MagnetronTecnicalTest.Tests.csproj"
if (-not (Test-Path $testProject)) {
    Write-Host "? Proyecto de pruebas no encontrado: $testProject" -ForegroundColor Red
    exit 1
}

try {
    # Restaurar dependencias
    Write-Host "?? Restaurando dependencias..." -ForegroundColor Blue
    dotnet restore $testProject
    if ($LASTEXITCODE -ne 0) {
        Write-Host "? Error restaurando dependencias" -ForegroundColor Red
        exit 1
    }

    # Compilar el proyecto de pruebas
    Write-Host "?? Compilando proyecto de pruebas..." -ForegroundColor Blue
    dotnet build $testProject --configuration $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Host "? Error compilando proyecto de pruebas" -ForegroundColor Red
        exit 1
    }

    # Preparar comando de test
    $testCommand = "dotnet test `"$testProject`" --configuration $Configuration --no-build --verbosity normal"
    
    if ($Filter) {
        $testCommand += " --filter `"$Filter`""
        Write-Host "?? Filtro aplicado: $Filter" -ForegroundColor Cyan
    }

    if ($Coverage) {
        Write-Host "?? Ejecutando con cobertura de código..." -ForegroundColor Blue
        
        # Crear directorio para reportes si no existe
        $coverageDir = "TestResults\Coverage"
        if (-not (Test-Path $coverageDir)) {
            New-Item -ItemType Directory -Path $coverageDir -Force | Out-Null
        }

        # Configurar coverlet con múltiples formatos
        $testCommand += " --collect:`"XPlat Code Coverage`""
        $testCommand += " --settings coverlet.runsettings"
        $testCommand += " -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover,cobertura,json,lcov"
        $testCommand += " -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude=`"[*.Tests]*,[*.TestResults]*`""
    }

    if ($Watch) {
        Write-Host "?? Ejecutando en modo watch..." -ForegroundColor Blue
        $testCommand += " --watch"
    }

    # Ejecutar pruebas
    Write-Host "?? Ejecutando pruebas..." -ForegroundColor Blue
    Write-Host "Comando: $testCommand" -ForegroundColor DarkGray
    
    Invoke-Expression $testCommand
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Todas las pruebas pasaron exitosamente" -ForegroundColor Green
        
        if ($Coverage) {
            Write-Host "?? Generando reporte de cobertura..." -ForegroundColor Blue
            
            # Instalar ReportGenerator si no está instalado
            $reportGeneratorPath = & dotnet tool list -g reportgenerator 2>$null
            if (-not $reportGeneratorPath -or $reportGeneratorPath -notmatch "reportgenerator") {
                Write-Host "?? Instalando ReportGenerator..." -ForegroundColor Blue
                dotnet tool install -g dotnet-reportgenerator-globaltool
            }

            # Buscar archivos de cobertura
            $coverageFiles = Get-ChildItem -Path "TestResults" -Filter "coverage.cobertura.xml" -Recurse
            if ($coverageFiles.Count -eq 0) {
                $coverageFiles = Get-ChildItem -Path "TestResults" -Filter "*.cobertura.xml" -Recurse
            }

            if ($coverageFiles.Count -gt 0) {
                $latestCoverage = $coverageFiles | Sort-Object LastWriteTime -Descending | Select-Object -First 1
                Write-Host "?? Archivo de cobertura encontrado: $($latestCoverage.FullName)" -ForegroundColor Cyan
                
                # Generar reporte HTML
                $reportPath = "TestResults\Coverage\Report"
                reportgenerator -reports:$($latestCoverage.FullName) -targetdir:$reportPath -reporttypes:"Html;HtmlSummary;Badges;TextSummary"
                
                Write-Host "?? Reporte de cobertura generado en: $reportPath" -ForegroundColor Green
                
                # Mostrar resumen en consola
                $summaryFile = Join-Path $reportPath "Summary.txt"
                if (Test-Path $summaryFile) {
                    Write-Host "`n=== RESUMEN DE COBERTURA ===" -ForegroundColor Yellow
                    Get-Content $summaryFile | Write-Host
                }

                if ($OpenReport) {
                    $indexPath = Join-Path $reportPath "index.html"
                    if (Test-Path $indexPath) {
                        Write-Host "?? Abriendo reporte en el navegador..." -ForegroundColor Blue
                        Start-Process $indexPath
                    }
                }
            } else {
                Write-Host "?? No se encontraron archivos de cobertura" -ForegroundColor Yellow
            }
        }
    } else {
        Write-Host "? Algunas pruebas fallaron" -ForegroundColor Red
        exit 1
    }

} catch {
    Write-Host "? Error ejecutando pruebas: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`n=== Resumen de pruebas ===" -ForegroundColor Green
Write-Host "Controladores probados:" -ForegroundColor Yellow
Write-Host "  ? PersonasController" -ForegroundColor Green
Write-Host "  ? ProductosController" -ForegroundColor Green  
Write-Host "  ? FacturasController" -ForegroundColor Green
Write-Host "  ? AuthController" -ForegroundColor Green
Write-Host "  ? ReportesController" -ForegroundColor Green
Write-Host "  ? Integration Tests" -ForegroundColor Green

Write-Host "`nTipos de pruebas incluidas:" -ForegroundColor Yellow
Write-Host "  ? Pruebas unitarias de controladores" -ForegroundColor Green
Write-Host "  ? Pruebas de validación de datos" -ForegroundColor Green
Write-Host "  ? Pruebas de casos de error" -ForegroundColor Green
Write-Host "  ? Pruebas de integración completa" -ForegroundColor Green
Write-Host "  ? Pruebas de autenticación JWT" -ForegroundColor Green

if ($Coverage) {
    Write-Host "`n?? Para ver el reporte detallado:" -ForegroundColor Cyan
    Write-Host "   Abrir: TestResults\Coverage\Report\index.html" -ForegroundColor White
    Write-Host "`n?? Para abrir automáticamente:" -ForegroundColor Cyan
    Write-Host "   .\run-tests.ps1 -Coverage -OpenReport" -ForegroundColor White
}