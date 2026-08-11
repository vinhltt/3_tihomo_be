#!/bin/bash

# TiHoMo Infrastructure Startup Script
# Starts all infrastructure services cho message queue testing

echo "🚀 Starting TiHoMo Infrastructure Services..."

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker Desktop first."
    exit 1
fi

# Navigate to BE directory
cd "$(dirname "$0")"

echo "📦 Pulling latest Docker images..."
docker-compose -f docker-compose.infrastructure.yml pull

echo "🏗️ Starting infrastructure services..."
docker-compose -f docker-compose.infrastructure.yml up -d

echo "⏳ Waiting for services to be ready..."

# Wait for PostgreSQL
echo "🐘 Waiting for PostgreSQL..."
until docker exec tihomo-postgres pg_isready -U tihomo -d TiHoMo_Dev > /dev/null 2>&1; do
    echo "  - PostgreSQL is starting up..."
    sleep 2
done
echo "✅ PostgreSQL is ready!"

# Wait for RabbitMQ
echo "🐰 Waiting for RabbitMQ..."
until docker exec tihomo-rabbitmq rabbitmqctl status > /dev/null 2>&1; do
    echo "  - RabbitMQ is starting up..."
    sleep 2
done
echo "✅ RabbitMQ is ready!"

# Wait for Loki
echo "📊 Waiting for Loki..."
until curl -f http://localhost:3100/ready > /dev/null 2>&1; do
    echo "  - Loki is starting up..."
    sleep 2
done
echo "✅ Loki is ready!"

# Wait for Prometheus
echo "📈 Waiting for Prometheus..."
until curl -f http://localhost:9090/-/healthy > /dev/null 2>&1; do
    echo "  - Prometheus is starting up..."
    sleep 2
done
echo "✅ Prometheus is ready!"

# Wait for Grafana
echo "📊 Waiting for Grafana..."
until curl -f http://localhost:3000/api/health > /dev/null 2>&1; do
    echo "  - Grafana is starting up..."
    sleep 2
done
echo "✅ Grafana is ready!"

echo ""
echo "🎉 All infrastructure services are ready!"
echo ""
echo "📋 Service URLs:"
echo "  🐰 RabbitMQ Management: http://localhost:15672 (tihomo/tihomo123)"
echo "  📊 Grafana Dashboard:   http://localhost:3000 (admin/admin123)"
echo "  📈 Prometheus:          http://localhost:9090"
echo "  📝 Loki:                http://localhost:3100"
echo ""
echo "💾 Database Connection:"
echo "  🐘 PostgreSQL:          localhost:5432"
echo "     CoreFinance DB:      TiHoMo_CoreFinance_Dev"
echo "     ExcelApi DB:         TiHoMo_ExcelApi_Dev"
echo "     Username:            tihomo"
echo "     Password:            tihomo123"
echo ""
echo "🔧 Next Steps:"
echo "  1. Start ExcelApi service: cd ExcelApi && dotnet run"
echo "  2. Start CoreFinance service: cd CoreFinance && dotnet run"
echo "  3. Test message queue flow by uploading Excel file"
echo ""
