# Build context is the repository root (see docker-compose.yml).
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
# gRPC (Cart/Order → Product) — its own port, see appsettings.Docker.json Kestrel:Endpoints:Grpc.
EXPOSE 8081
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Central Package Management (Directory.Packages.props) and shared build settings
# (Directory.Build.props) must be present before restore, since every PackageReference here
# omits a Version attribute and relies on these files to resolve one.
COPY ["Directory.Packages.props", "Directory.Build.props", "./"]
COPY ["src/Services/Product/Product.Api/Product.Api.csproj", "src/Services/Product/Product.Api/"]
COPY ["src/Services/Product/Product.Application/Product.Application.csproj", "src/Services/Product/Product.Application/"]
COPY ["src/Services/Product/Product.Domain/Product.Domain.csproj", "src/Services/Product/Product.Domain/"]
COPY ["src/Services/Product/Product.Infrastructure/Product.Infrastructure.csproj", "src/Services/Product/Product.Infrastructure/"]
COPY ["src/BuildingBlocks/Contracts/ECommerce.Contracts.csproj", "src/BuildingBlocks/Contracts/"]
COPY ["src/BuildingBlocks/Application/ECommerce.BuildingBlocks.Application.csproj", "src/BuildingBlocks/Application/"]
COPY ["src/BuildingBlocks/WebApi/ECommerce.BuildingBlocks.WebApi.csproj", "src/BuildingBlocks/WebApi/"]
RUN dotnet restore "src/Services/Product/Product.Api/Product.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Product/Product.Api"
RUN dotnet build "Product.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Product.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Product.Api.dll"]
