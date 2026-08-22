# Build context is the repository root (see docker-compose.yml).
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Packages.props", "Directory.Build.props", "./"]
COPY ["src/Services/Payment/Payment.Api/Payment.Api.csproj", "src/Services/Payment/Payment.Api/"]
COPY ["src/Services/Payment/Payment.Application/Payment.Application.csproj", "src/Services/Payment/Payment.Application/"]
COPY ["src/Services/Payment/Payment.Domain/Payment.Domain.csproj", "src/Services/Payment/Payment.Domain/"]
COPY ["src/Services/Payment/Payment.Infrastructure/Payment.Infrastructure.csproj", "src/Services/Payment/Payment.Infrastructure/"]
COPY ["src/BuildingBlocks/Contracts/ECommerce.Contracts.csproj", "src/BuildingBlocks/Contracts/"]
COPY ["src/BuildingBlocks/Application/ECommerce.BuildingBlocks.Application.csproj", "src/BuildingBlocks/Application/"]
COPY ["src/BuildingBlocks/WebApi/ECommerce.BuildingBlocks.WebApi.csproj", "src/BuildingBlocks/WebApi/"]
RUN dotnet restore "src/Services/Payment/Payment.Api/Payment.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Payment/Payment.Api"
RUN dotnet build "Payment.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Payment.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Payment.Api.dll"]
