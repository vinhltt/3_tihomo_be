# Dockerfile for ExcelApi service
# Using multi-stage build pattern for optimized container size

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set working directory for the build
WORKDIR /src

# Copy project files first for better layer caching
COPY ["ExcelApi/ExcelApi.csproj", "ExcelApi/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.projitems", "Shared/Shared.Contracts/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.shproj", "Shared/Shared.Contracts/"]

# Restore dependencies
RUN dotnet restore "ExcelApi/ExcelApi.csproj"

# Copy all source files
COPY . .

# Build the application
WORKDIR "/src/ExcelApi"
RUN dotnet build "ExcelApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
# Publish the application
WORKDIR "/src/ExcelApi"
RUN dotnet publish "ExcelApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser

# Set working directory
WORKDIR /app

# Copy published files from publish stage
COPY --from=publish /app/publish .

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

# Set the entry point
ENTRYPOINT ["dotnet", "ExcelApi.dll"]

# Image metadata
LABEL maintainer="VinhLTT <vinhltt.dev@gmail.com>"
LABEL description="Excel API service for processing Excel and CSV files"