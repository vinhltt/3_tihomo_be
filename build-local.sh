#!/bin/bash
# Local Docker build script matching CI/CD context
# Usage: ./build-local.sh [service-name|all] [--no-cache]

set -e

# Color output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Get service name from argument
SERVICE=${1:-all}
NO_CACHE_FLAG=""

# Check for --no-cache flag
if [[ "$2" == "--no-cache" ]] || [[ "$1" == "--no-cache" ]]; then
    NO_CACHE_FLAG="--no-cache"
    echo -e "${YELLOW}Building with --no-cache flag${NC}"
fi

# Function to build service
build_service() {
    local service=$1
    local dockerfile_path=""
    local image_name=""
    
    case $service in
        identity-api)
            dockerfile_path="Identity/Dockerfile"
            image_name="tihomo-identity-api:local"
            ;;
        corefinance-api)
            dockerfile_path="CoreFinance/Dockerfile"
            image_name="tihomo-corefinance-api:local"
            ;;
        excel-api)
            dockerfile_path="ExcelApi/Dockerfile"
            image_name="tihomo-excel-api:local"
            ;;
        ocelot-gateway)
            dockerfile_path="Ocelot.Gateway/Dockerfile"
            image_name="tihomo-ocelot-gateway:local"
            ;;
        *)
            echo -e "${RED}Unknown service: $service${NC}"
            return 1
            ;;
    esac
    
    echo -e "${GREEN}Building $service...${NC}"
    echo "Dockerfile: $dockerfile_path"
    echo "Image: $image_name"
    
    # Build with same context as CI/CD (from src/be/ directory)
    docker build $NO_CACHE_FLAG -f "$dockerfile_path" -t "$image_name" .
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ $service built successfully${NC}"
    else
        echo -e "${RED}✗ Failed to build $service${NC}"
        return 1
    fi
}

# Main execution
echo "========================================="
echo "TiHoMo Local Docker Build (CI/CD Context)"
echo "========================================="
echo "Build context: $(pwd)"
echo ""

if [ "$SERVICE" == "all" ]; then
    echo -e "${YELLOW}Building all services...${NC}"
    services=("identity-api" "corefinance-api" "excel-api" "ocelot-gateway")
    
    for svc in "${services[@]}"; do
        echo ""
        build_service "$svc"
    done
    
    echo ""
    echo -e "${GREEN}All services built successfully!${NC}"
else
    build_service "$SERVICE"
fi

echo ""
echo "Built images:"
docker images | grep "tihomo-.*local"