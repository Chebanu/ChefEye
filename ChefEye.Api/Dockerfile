# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ChefEye.Api/ChefEye.Api.csproj", "ChefEye.Api/"]
COPY ["ChefEye.Domain/ChefEye.Domain.csproj", "ChefEye.Domain/"]
COPY ["ChefEye.Contracts/ChefEye.Contracts.csproj", "ChefEye.Contracts/"]
RUN dotnet restore "./ChefEye.Api/ChefEye.Api.csproj"
COPY . .
WORKDIR "/src/ChefEye.Api"
RUN dotnet build "./ChefEye.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ChefEye.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChefEye.Api.dll"]