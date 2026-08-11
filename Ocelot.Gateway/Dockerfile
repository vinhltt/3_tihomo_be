# Dockerfile for TiHoMo Ocelot Gateway
# Multi-stage build để tối ưu hóa kích thước image

# Stage 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set working directory
WORKDIR /src

# Copy project file
COPY ["Ocelot.Gateway/Ocelot.Gateway.csproj", "Ocelot.Gateway/"]

# Restore dependencies
RUN dotnet restore "Ocelot.Gateway/Ocelot.Gateway.csproj"

# Copy all source code
COPY . .

# Build và publish application
WORKDIR "/src/Ocelot.Gateway"
RUN dotnet build "Ocelot.Gateway.csproj" -c Release -o /app/build
RUN dotnet publish "Ocelot.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

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
ENTRYPOINT ["dotnet", "Ocelot.Gateway.dll"]
