# Script para construir y ejecutar la aplicación Magnetron con Docker
# build-and-run.ps1

param(
    [string]$Action = "build-and-run",
    [string]$Environment = "Development"
)

Write-Host "=== Magnetron Technical Test - Docker Build Script ===" -ForegroundColor Green
Write-Host "Acción: $Action" -ForegroundColor Yellow
Write-Host "Entorno: $Environment" -ForegroundColor Yellow

switch ($Action.ToLower()) {
    "build" {
        Write-Host "Construyendo imagen Docker..." -ForegroundColor Blue
        docker build -t magnetron-api:latest .
        if ($LASTEXITCODE -eq 0) {
            Write-Host "? Imagen construida exitosamente" -ForegroundColor Green
        } else {
            Write-Host "? Error construyendo la imagen" -ForegroundColor Red
            exit 1
        }
    }
    
    "run" {
        Write-Host "Ejecutando contenedores..." -ForegroundColor Blue
        docker-compose up -d
        if ($LASTEXITCODE -eq 0) {
            Write-Host "? Contenedores ejecutándose" -ForegroundColor Green
            Write-Host "API disponible en: http://localhost:8080" -ForegroundColor Cyan
            Write-Host "Swagger UI: http://localhost:8080" -ForegroundColor Cyan
        } else {
            Write-Host "? Error ejecutando contenedores" -ForegroundColor Red
            exit 1
        }
    }
    
    "build-and-run" {
        Write-Host "Construyendo y ejecutando..." -ForegroundColor Blue
        
        # Detener contenedores existentes
        docker-compose down
        
        # Construir imagen
        docker build -t magnetron-api:latest .
        if ($LASTEXITCODE -ne 0) {
            Write-Host "? Error construyendo la imagen" -ForegroundColor Red
            exit 1
        }
        
        # Ejecutar contenedores
        docker-compose up -d
        if ($LASTEXITCODE -eq 0) {
            Write-Host "? Aplicación desplegada exitosamente" -ForegroundColor Green
            Write-Host "API disponible en: http://localhost:8080" -ForegroundColor Cyan
            Write-Host "Swagger UI: http://localhost:8080" -ForegroundColor Cyan
            Write-Host "PostgreSQL: localhost:5432" -ForegroundColor Cyan
        } else {
            Write-Host "? Error ejecutando contenedores" -ForegroundColor Red
            exit 1
        }
    }
    
    "stop" {
        Write-Host "Deteniendo contenedores..." -ForegroundColor Blue
        docker-compose down
        Write-Host "? Contenedores detenidos" -ForegroundColor Green
    }
    
    "logs" {
        Write-Host "Mostrando logs de la aplicación..." -ForegroundColor Blue
        docker-compose logs -f api
    }
    
    "clean" {
        Write-Host "Limpiando contenedores y volúmenes..." -ForegroundColor Blue
        docker-compose down -v
        docker system prune -f
        Write-Host "? Limpieza completada" -ForegroundColor Green
    }
    
    default {
        Write-Host "Uso: .\build-and-run.ps1 -Action [build|run|build-and-run|stop|logs|clean]" -ForegroundColor Yellow
        Write-Host "Ejemplo: .\build-and-run.ps1 -Action build-and-run" -ForegroundColor Yellow
    }
}