# Build context is the repository root (see docker-compose.yml).
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Packages.props", "Directory.Build.props", "./"]
COPY ["src/Services/Notification/Notification.Api/Notification.Api.csproj", "src/Services/Notification/Notification.Api/"]
COPY ["src/BuildingBlocks/Contracts/ECommerce.Contracts.csproj", "src/BuildingBlocks/Contracts/"]
RUN dotnet restore "src/Services/Notification/Notification.Api/Notification.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Notification/Notification.Api"
RUN dotnet build "Notification.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Notification.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notification.Api.dll"]
