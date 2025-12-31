# E2E Test Runner with Playwright
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Install PowerShell for Playwright
RUN apt-get update && apt-get install -y \
    curl \
    && curl -fsSL https://packages.microsoft.com/keys/microsoft.asc | tee /etc/apt/trusted.gpg.d/microsoft.asc > /dev/null \
    && echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-debian-bookworm-prod bookworm main" > /etc/apt/sources.list.d/microsoft.list \
    && apt-get update \
    && apt-get install -y powershell \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Copy solution and project files
COPY *.sln ./
COPY src/PayGoHub.Domain/*.csproj src/PayGoHub.Domain/
COPY src/PayGoHub.Application/*.csproj src/PayGoHub.Application/
COPY src/PayGoHub.Infrastructure/*.csproj src/PayGoHub.Infrastructure/
COPY src/PayGoHub.Web/*.csproj src/PayGoHub.Web/
COPY tests/PayGoHub.Tests/*.csproj tests/PayGoHub.Tests/
COPY tests/PayGoHub.E2E/*.csproj tests/PayGoHub.E2E/
RUN dotnet restore tests/PayGoHub.E2E

# Copy all source code
COPY . .

# Build E2E tests
RUN dotnet build tests/PayGoHub.E2E -c Release --no-restore

# Install Playwright browsers
RUN pwsh tests/PayGoHub.E2E/bin/Release/net10.0/playwright.ps1 install --with-deps chromium

# Set environment variables
ENV BASE_URL=http://web:8080

# Run tests
ENTRYPOINT ["dotnet", "test", "tests/PayGoHub.E2E", "-c", "Release", "--no-build", "--logger:trx"]
