# =============================================================================
# Dockerfile for ProductManagementSystem API
# Multi-stage build for optimized production image
# =============================================================================

# -----------------------------------------------------------------------------
# Stage 1: Build
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution and project files first (for better layer caching)
COPY ProductManagementSystem.sln .
COPY ProductManagementSystem.Application/ProductManagementSystem.Application.csproj ProductManagementSystem.Application/

# Restore dependencies (this layer will be cached if project files don't change)
# Using BuildKit cache mount for NuGet packages
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore --verbosity quiet

# Copy all source code
COPY . .

# Build and publish with optimizations
WORKDIR /src/ProductManagementSystem.Application
RUN dotnet publish \
    -c ${BUILD_CONFIGURATION} \
    -o /app/publish \
    --no-restore \
    --no-self-contained \
    /p:PublishTrimmed=false \
    /p:PublishSingleFile=false \
    /p:EnableCompressionInSingleFile=false

# -----------------------------------------------------------------------------
# Stage 2: Runtime
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for health checks (lightweight alternative to wget)
# Using --mount=type=cache for better layer caching
RUN --mount=type=cache,target=/var/cache/apt,sharing=locked \
    apt-get update && \
    apt-get install -y --no-install-recommends curl && \
    rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r appgroup && \
    useradd -r -g appgroup -u 1000 appuser && \
    mkdir -p /app/logs && \
    chown -R appuser:appgroup /app

# Copy published application
COPY --from=build --chown=appuser:appgroup /app/publish .

# Switch to non-root user
USER appuser

# Environment variables
# DOTNET_RUNNING_IN_CONTAINER is set for .NET optimization in containers
# ASPNETCORE_HTTP_PORTS will be set at runtime via APP_PORT secret
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Health check - checks if the application is responding via dedicated /health endpoint
# Uses ASPNETCORE_HTTP_PORTS environment variable (set at runtime via APP_PORT secret)
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD sh -c 'curl -f http://localhost:${ASPNETCORE_HTTP_PORTS}/health || exit 1'

# Entry point
ENTRYPOINT ["dotnet", "ProductManagementSystem.Application.dll"]

