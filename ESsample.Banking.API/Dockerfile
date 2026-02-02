# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8082
EXPOSE 8083

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /src
COPY ["EventBus/EventBus.csproj", "EventBus/"]
COPY ["EventBus.Kafka/EventBus.Kafka.csproj", "EventBus.Kafka/"]
COPY ["EventSourcing/EventSourcing.csproj", "EventSourcing/"]
COPY ["EventSourcing.Postgres/EventSourcing.Postgres.csproj", "EventSourcing.Postgres/"]
COPY ["ESsample.Banking.Shared/ESsample.Banking.Shared.csproj", "ESsample.Banking.Shared/"]
COPY ["ESsample.Banking.API/ESsample.Banking.API.csproj", "ESsample.Banking.API/"]
RUN dotnet restore "./ESsample.Banking.API/ESsample.Banking.API.csproj"
COPY . .
WORKDIR "/src/ESsample.Banking.API"
RUN dotnet build "./ESsample.Banking.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Debug
RUN dotnet publish "./ESsample.Banking.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ESsample.Banking.API.dll"]
