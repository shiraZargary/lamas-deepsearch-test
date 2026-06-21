# Multi-stage build for the DeepSearch .NET 9 Web API.
# Build context must be the `server/` folder (see render.yaml / docker build below).

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore first (better layer caching) using the solution.
COPY DeepSearch.sln ./
COPY DeepSearch.Api/DeepSearch.Api.csproj           DeepSearch.Api/
COPY DeepSearch.Application/DeepSearch.Application.csproj       DeepSearch.Application/
COPY DeepSearch.Domain/DeepSearch.Domain.csproj         DeepSearch.Domain/
COPY DeepSearch.Infrastructure/DeepSearch.Infrastructure.csproj DeepSearch.Infrastructure/
COPY DeepSearch.UnitTests/DeepSearch.UnitTests.csproj      DeepSearch.UnitTests/
RUN dotnet restore DeepSearch.Api/DeepSearch.Api.csproj

# Copy the rest and publish the API.
COPY . .
RUN dotnet publish DeepSearch.Api/DeepSearch.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Production by default; the platform injects PORT (handled in Program.cs).
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "DeepSearch.Api.dll"]
