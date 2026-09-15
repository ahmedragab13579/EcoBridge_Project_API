# Multi-stage Dockerfile for EcoBridge API (.NET 10)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for caching restore layer
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["EcoBridgeAPI/EcoBridgeAPI.csproj", "EcoBridgeAPI/"]

RUN dotnet restore "EcoBridgeAPI/EcoBridgeAPI.csproj"

# Copy all source code and build
COPY . .
WORKDIR "/src/EcoBridgeAPI"
RUN dotnet build "EcoBridgeAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EcoBridgeAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EcoBridgeAPI.dll"]
