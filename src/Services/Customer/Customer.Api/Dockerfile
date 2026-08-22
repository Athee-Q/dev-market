# Build context is the repository root (see docker-compose.yml).
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Packages.props", "Directory.Build.props", "./"]
COPY ["src/Services/Customer/Customer.Api/Customer.Api.csproj", "src/Services/Customer/Customer.Api/"]
COPY ["src/Services/Customer/Customer.Application/Customer.Application.csproj", "src/Services/Customer/Customer.Application/"]
COPY ["src/Services/Customer/Customer.Domain/Customer.Domain.csproj", "src/Services/Customer/Customer.Domain/"]
COPY ["src/Services/Customer/Customer.Infrastructure/Customer.Infrastructure.csproj", "src/Services/Customer/Customer.Infrastructure/"]
COPY ["src/BuildingBlocks/Contracts/ECommerce.Contracts.csproj", "src/BuildingBlocks/Contracts/"]
COPY ["src/BuildingBlocks/Application/ECommerce.BuildingBlocks.Application.csproj", "src/BuildingBlocks/Application/"]
COPY ["src/BuildingBlocks/WebApi/ECommerce.BuildingBlocks.WebApi.csproj", "src/BuildingBlocks/WebApi/"]
RUN dotnet restore "src/Services/Customer/Customer.Api/Customer.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Customer/Customer.Api"
RUN dotnet build "Customer.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Customer.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Customer.Api.dll"]
