# Build context is the repository root (see docker-compose.yml).
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Packages.props", "Directory.Build.props", "./"]
COPY ["src/Services/Order/Order.Api/Order.Api.csproj", "src/Services/Order/Order.Api/"]
COPY ["src/Services/Order/Order.Application/Order.Application.csproj", "src/Services/Order/Order.Application/"]
COPY ["src/Services/Order/Order.Domain/Order.Domain.csproj", "src/Services/Order/Order.Domain/"]
COPY ["src/Services/Order/Order.Infrastructure/Order.Infrastructure.csproj", "src/Services/Order/Order.Infrastructure/"]
COPY ["src/BuildingBlocks/Contracts/ECommerce.Contracts.csproj", "src/BuildingBlocks/Contracts/"]
COPY ["src/BuildingBlocks/Application/ECommerce.BuildingBlocks.Application.csproj", "src/BuildingBlocks/Application/"]
COPY ["src/BuildingBlocks/WebApi/ECommerce.BuildingBlocks.WebApi.csproj", "src/BuildingBlocks/WebApi/"]
RUN dotnet restore "src/Services/Order/Order.Api/Order.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Order/Order.Api"
RUN dotnet build "Order.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Order.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Order.Api.dll"]
