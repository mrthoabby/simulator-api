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

COPY ProductManagementSystem.sln .
COPY ProductManagementSystem.Application/ProductManagementSystem.Application.csproj ProductManagementSystem.Application/

RUN dotnet restore

COPY . .

WORKDIR /src/ProductManagementSystem.Application
RUN dotnet publish \
    -c ${BUILD_CONFIGURATION} \
    -o /app/publish \
    --no-self-contained \
    /p:PublishTrimmed=false \
    /p:PublishSingleFile=false \
    /p:EnableCompressionInSingleFile=false

# -----------------------------------------------------------------------------
# Stage 2: Runtime
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN apt-get update && \
    apt-get install -y --no-install-recommends curl && \
    rm -rf /var/lib/apt/lists/*

RUN groupadd -r appgroup && \
    useradd -r -g appgroup -u 1000 appuser && \
    mkdir -p /app/logs && \
    chown -R appuser:appgroup /app

COPY --from=build --chown=appuser:appgroup /app/publish .

USER appuser

ENV DOTNET_RUNNING_IN_CONTAINER=true

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD sh -c 'curl -f http://localhost:${ASPNETCORE_HTTP_PORTS}/health || exit 1'

ENTRYPOINT ["dotnet", "ProductManagementSystem.Application.dll"]
