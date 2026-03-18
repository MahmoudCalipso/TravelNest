# Multi-stage build for TravelNest API
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["TravelNest.sln", "."]
COPY ["TravelNest.API/TravelNest.API.csproj", "TravelNest.API/"]
COPY ["TravelNest.Application/TravelNest.Application.csproj", "TravelNest.Application/"]
COPY ["TravelNest.Domain/TravelNest.Domain.csproj", "TravelNest.Domain/"]
COPY ["TravelNest.Infrastructure/TravelNest.Infrastructure.csproj", "TravelNest.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "TravelNest.sln"

# Copy source code
COPY . .

# Build the project
RUN dotnet build "TravelNest.sln" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "TravelNest.API/TravelNest.API.csproj" -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 5000

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "TravelNest.API.dll"]
