# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install Node.js (required for Angular build)
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get install -y nodejs

# Copy csproj and restore as distinct layers
COPY WebApi.csproj .
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish WebApi.csproj -c Release -o /app/publish /p:UseAppHost=false /p:RUN_NPM=true

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Default .NET 8 port is 8080, but user requested 80
ENV ASPNETCORE_HTTP_PORTS=80
EXPOSE 80

ENTRYPOINT ["dotnet", "WebApi.dll"]