# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
# Предустановка curl внутри линукса
USER root
# Обновление и установка curl
RUN apk update && apk add --no-cache curl
USER $APP_UID
WORKDIR /app


# HTTP и HTTPS порты
EXPOSE 5507 5508 

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Dekauto.Auth.Service.csproj", "."]
RUN dotnet restore "./Dekauto.Auth.Service.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./Dekauto.Auth.Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Dekauto.Auth.Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dekauto.Auth.Service.dll"]