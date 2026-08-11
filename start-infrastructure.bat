@echo off
REM TiHoMo Infrastructure Startup Script for Windows
REM Starts all infrastructure services cho message queue testing

echo 🚀 Starting TiHoMo Infrastructure Services...

REM Check if Docker is running
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Docker is not running. Please start Docker Desktop first.
    pause
    exit /b 1
)

REM Navigate to current directory
cd /d "%~dp0"

echo 📦 Pulling latest Docker images...
docker-compose -f docker-compose.infrastructure.yml pull

echo 🏗️ Starting infrastructure services...
docker-compose -f docker-compose.infrastructure.yml up -d

echo ⏳ Waiting for services to be ready...

REM Wait for PostgreSQL
echo 🐘 Waiting for PostgreSQL...
:wait_postgres
docker exec tihomo-postgres pg_isready -U tihomo -d TiHoMo_Dev >nul 2>&1
if %errorlevel% neq 0 (
    echo   - PostgreSQL is starting up...
    timeout /t 2 >nul
    goto wait_postgres
)
echo ✅ PostgreSQL is ready!

REM Wait for RabbitMQ
echo 🐰 Waiting for RabbitMQ...
:wait_rabbitmq
docker exec tihomo-rabbitmq rabbitmqctl status >nul 2>&1
if %errorlevel% neq 0 (
    echo   - RabbitMQ is starting up...
    timeout /t 2 >nul
    goto wait_rabbitmq
)
echo ✅ RabbitMQ is ready!

REM Wait for Loki
echo 📊 Waiting for Loki...
:wait_loki
curl -f http://localhost:3100/ready >nul 2>&1
if %errorlevel% neq 0 (
    echo   - Loki is starting up...
    timeout /t 2 >nul
    goto wait_loki
)
echo ✅ Loki is ready!

REM Wait for Prometheus
echo 📈 Waiting for Prometheus...
:wait_prometheus
curl -f http://localhost:9090/-/healthy >nul 2>&1
if %errorlevel% neq 0 (
    echo   - Prometheus is starting up...
    timeout /t 2 >nul
    goto wait_prometheus
)
echo ✅ Prometheus is ready!

REM Wait for Grafana
echo 📊 Waiting for Grafana...
:wait_grafana
curl -f http://localhost:3000/api/health >nul 2>&1
if %errorlevel% neq 0 (
    echo   - Grafana is starting up...
    timeout /t 2 >nul
    goto wait_grafana
)
echo ✅ Grafana is ready!

echo.
echo 🎉 All infrastructure services are ready!
echo.
echo 📋 Service URLs:
echo   🐰 RabbitMQ Management: http://localhost:15672 (tihomo/tihomo123)
echo   📊 Grafana Dashboard:   http://localhost:3000 (admin/admin123)
echo   📈 Prometheus:          http://localhost:9090
echo   📝 Loki:                http://localhost:3100
echo.
echo 💾 Database Connection:
echo   🐘 PostgreSQL:          localhost:5432
echo      CoreFinance DB:      TiHoMo_CoreFinance_Dev
echo      ExcelApi DB:         TiHoMo_ExcelApi_Dev
echo      Username:            tihomo
echo      Password:            tihomo123
echo.
echo 🔧 Next Steps:
echo   1. Start ExcelApi service: cd ExcelApi ^&^& dotnet run
echo   2. Start CoreFinance service: cd CoreFinance ^&^& dotnet run
echo   3. Test message queue flow by uploading Excel file
echo.
pause
