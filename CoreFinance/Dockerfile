# Dockerfile for TiHoMo CoreFinance API
# Multi-stage build để tối ưu hóa kích thước image

# Stage 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set working directory
WORKDIR /src

# Copy solution file và project files cho dependency resolution
COPY ["CoreFinance/CoreFinance.sln", "CoreFinance/"]
COPY ["CoreFinance/CoreFinance.Api/CoreFinance.Api.csproj", "CoreFinance/CoreFinance.Api/"]
COPY ["CoreFinance/CoreFinance.Application/CoreFinance.Application.csproj", "CoreFinance/CoreFinance.Application/"]
COPY ["CoreFinance/CoreFinance.Domain/CoreFinance.Domain.csproj", "CoreFinance/CoreFinance.Domain/"]  
COPY ["CoreFinance/CoreFinance.Infrastructure/CoreFinance.Infrastructure.csproj", "CoreFinance/CoreFinance.Infrastructure/"]
COPY ["CoreFinance/CoreFinance.Contracts/CoreFinance.Contracts.csproj", "CoreFinance/CoreFinance.Contracts/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.projitems", "Shared/Shared.Contracts/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.shproj", "Shared/Shared.Contracts/"]

# Restore dependencies
RUN dotnet restore "CoreFinance/CoreFinance.Api/CoreFinance.Api.csproj"

# Copy all source code
COPY . .

# Build và publish application
WORKDIR "/src/CoreFinance/CoreFinance.Api"
RUN dotnet build "CoreFinance.Api.csproj" -c Release -o /app/build
RUN dotnet publish "CoreFinance.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser

# Set working directory
WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appuser /app
USER appuser

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Start the application
ENTRYPOINT ["dotnet", "CoreFinance.Api.dll"]
