# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Install Node.js 20
RUN apt-get update \
 && apt-get install -y curl \
 && curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
 && apt-get install -y nodejs \
 && rm -rf /var/lib/apt/lists/*

WORKDIR /src

# Restore dependencies
COPY AccuScraperWebWithReact.sln ./
COPY AccuScraperWebWithReact.Server/AccuScraperWebWithReact.Server.csproj \
     AccuScraperWebWithReact.Server/
COPY accuscraperwebwithreact.client/package.json \
     accuscraperwebwithreact.client/
COPY accuscraperwebwithreact.client/package-lock.json \
     accuscraperwebwithreact.client/

RUN dotnet restore AccuScraperWebWithReact.Server/AccuScraperWebWithReact.Server.csproj

# Copy full source and publish
COPY AccuScraperWebWithReact.Server/ AccuScraperWebWithReact.Server/
COPY accuscraperwebwithreact.client/ accuscraperwebwithreact.client/

WORKDIR /src/AccuScraperWebWithReact.Server

RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app
COPY --from=build /app/publish .

# Render injects $PORT at runtime, default 10000
# ASP.NET Core respects ASPNETCORE_HTTP_PORTS automatically
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "AccuScraperWebWithReact.Server.dll"]
