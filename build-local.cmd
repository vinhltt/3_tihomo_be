@echo off
REM Local Docker build script for Windows matching CI/CD context
REM Usage: build-local.cmd [service-name|all] [--no-cache]

setlocal enabledelayedexpansion

set "SERVICE=%~1"
set "NO_CACHE_FLAG="

REM Default to 'all' if no service specified
if "%SERVICE%"=="" set "SERVICE=all"

REM Check for --no-cache flag
if "%~1"=="--no-cache" (
    set "NO_CACHE_FLAG=--no-cache"
    set "SERVICE=all"
    echo Building with --no-cache flag
) else if "%~2"=="--no-cache" (
    set "NO_CACHE_FLAG=--no-cache"
    echo Building with --no-cache flag
)

echo =========================================
echo TiHoMo Local Docker Build (CI/CD Context)
echo =========================================
echo Build context: %CD%
echo.

if "%SERVICE%"=="all" goto build_all

REM Build single service
call :build_service "%SERVICE%"
goto end

:build_all
echo Building all services...
echo.
call :build_service "identity-api"
echo.
call :build_service "corefinance-api"
echo.
call :build_service "excel-api"
echo.
call :build_service "ocelot-gateway"
echo.
echo All services built successfully!
goto show_images

:build_service
set "service=%~1"
set "dockerfile_path="
set "image_name="

if "%service%"=="identity-api" (
    set "dockerfile_path=Identity/Dockerfile"
    set "image_name=tihomo-identity-api:local"
) else if "%service%"=="corefinance-api" (
    set "dockerfile_path=CoreFinance/Dockerfile"
    set "image_name=tihomo-corefinance-api:local"
) else if "%service%"=="excel-api" (
    set "dockerfile_path=ExcelApi/Dockerfile"
    set "image_name=tihomo-excel-api:local"
) else if "%service%"=="ocelot-gateway" (
    set "dockerfile_path=Ocelot.Gateway/Dockerfile"
    set "image_name=tihomo-ocelot-gateway:local"
) else (
    echo Unknown service: %service%
    exit /b 1
)

echo Building %service%...
echo Dockerfile: %dockerfile_path%
echo Image: %image_name%

REM Build with same context as CI/CD (from src/be/ directory)
docker build %NO_CACHE_FLAG% -f "%dockerfile_path%" -t "%image_name%" .

if errorlevel 1 (
    echo Failed to build %service%
    exit /b 1
) else (
    echo [✓] %service% built successfully
)
goto :eof

:show_images
echo.
echo Built images:
docker images --filter "reference=tihomo-*:local"

:end
echo.
echo Build completed!