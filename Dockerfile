# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy project files for restore
COPY src/PayGoHub.Domain/*.csproj src/PayGoHub.Domain/
COPY src/PayGoHub.Application/*.csproj src/PayGoHub.Application/
COPY src/PayGoHub.Infrastructure/*.csproj src/PayGoHub.Infrastructure/
COPY src/PayGoHub.Web/*.csproj src/PayGoHub.Web/

# Restore dependencies (only for src projects)
RUN dotnet restore src/PayGoHub.Web/PayGoHub.Web.csproj

# Copy source code and build
COPY src/ src/
RUN dotnet publish src/PayGoHub.Web -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=build /app/publish .

# Create non-root user
RUN groupadd --gid 1001 appuser && \
    useradd --uid 1001 --gid 1001 --shell /bin/false appuser && \
    chown -R appuser:appuser /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=30s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "PayGoHub.Web.dll"]
